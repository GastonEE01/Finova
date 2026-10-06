# Plan técnico — Dashboard (006-dashboard)

Spec base: `specs/006-dashboard/spec.md` (APROBADA — no modificar).
Decisiones aprobadas respetadas: período = mes calendario (día 1 a hoy); totales agrupados por moneda sin conversión; últimos 5 movimientos; página dedicada `/dashboard`.

## 1. Enfoque general

Agregar un endpoint de lectura agregada `GET /api/dashboard` (solo lectura, sin cambios de modelo ni migraciones) que calcule todo en el backend con `decimal`, y una página `/dashboard` en Next.js/MUI que lo consuma con el patrón `apiFetch` existente. Reutilizar los patrones de `MovementService` (aislamiento vía `m.Account.UserId == userId`), `AccountService` (saldo derivado ingresos − gastos) y `MovementsController` (`[Authorize]` + `GetUserId()` del claim `sub`).

No se crean entidades, tablas ni migraciones. No se toca el historial existente ni el cálculo de saldos.

## 2. Cambios backend

### 2.1 Archivos nuevos (Application)
- `backend/Finova.Application/DTOs/DashboardResponse.cs`
  - Responsabilidad: contrato del endpoint, en inglés (constitución §5).
  - Forma propuesta:
    - `List<CurrencyTotal> TotalBalances` (saldo total por moneda = suma de saldos de cuentas del usuario).
    - `List<CurrencyTotal> TotalIncome` (ingresos del mes por moneda).
    - `List<CurrencyTotal> TotalExpenses` (gastos del mes por moneda).
    - `List<MovementHistoryResponse> RecentMovements` (reutiliza DTO existente: fecha, tipo, monto, cuenta, categoría).
    - `CurrencyTotal { string Currency; decimal Amount; }` (puede ir como clase anidada o en el mismo archivo).
    - Opcional: `string PeriodFrom, PeriodTo` o `DateTime` para transparencia del período (día 1 → hoy UTC). Ayuda a validar criterio "montos coinciden con historial".
- `backend/Finova.Application/Interfaces/IDashboardService.cs`
  - Responsabilidad: `Task<DashboardResponse> GetAsync(Guid userId);` Sin parámetros de período (V1 = mes actual fijo, fuera de alcance filtros personalizados).

### 2.2 Archivos nuevos (Infrastructure)
- `backend/Finova.Infrastructure/Services/DashboardService.cs : IDashboardService`
  - Responsabilidad: los 3 cálculos, todo con `decimal`, siempre filtrado por `userId`.
  - Lógica:
    1. Período: `now = DateTime.UtcNow`; `from = new DateTime(now.Year, now.Month, 1, 0,0,0, DateTimeKind.Utc)`; `to = now` (comparación por fecha completa, no solo `.Date`, para incluir movimientos futuros dentro del mes según spec — los movimientos con fecha posterior a hoy pero dentro del mismo mes SÍ cuentan; solo se excluyen los de otro mes).
    2. `TotalBalances`: query sobre `Accounts.Where(a => a.UserId == userId)` con `Balance = a.Movements.Sum(...)` (igual que `AccountService.GetAllAsync`), luego agrupar en memoria por `Currency` sumando balances. Incluir cuentas sin movimientos (aportan 0).
    3. `TotalIncome / TotalExpenses`: query sobre `Movements.Where(m => m.Account.UserId == userId && m.Date >= from && m.Date.Month == now.Month && m.Date.Year == now.Year)` — condición de mes calendario explícita para no arrastrar meses anteriores; separar por `Type` y agrupar por `m.Account.Currency` sumando `Amount`. Comparar fechas en UTC (los movimientos se guardan con `DateTimeKind.Utc` en `MovementService.CreateAsync`).
    4. `RecentMovements`: top 5 `OrderByDescending(m => m.Date).ThenByDescending(m => m.Id)`, proyección a `MovementHistoryResponse` (sin `RunningBalance` o con 0/no incluido — el saldo por fila no es requisito del dashboard; reutilizar el DTO evita un DTO nuevo pero debe documentarse que `RunningBalance` no aplica aquí; alternativa: proyectar a `MovementResponse` + `AccountName/Currency`).

### 2.3 Archivos a modificar (API)
- `backend/Finova.API/Controllers/DashboardController.cs` (NUEVO, preferido) o agregar `GET /api/movements/dashboard` a `MovementsController.cs` (alternativa).
  - Decisión: **nuevo `DashboardController`** con `[Authorize]` y `GetUserId()` copiado del patrón de `MovementsController`. Motivo: el dashboard agrega cuentas + movimientos, no pertenece solo a movimientos; evita engordar `MovementsController`.
  - `GET /api/dashboard` → `200 DashboardResponse`; sin JWT → `401` automático por `[Authorize]` (requisito spec).
- `backend/Finova.API/Program.cs` (MODIFICAR): registrar `IDashboardService → DashboardService` en DI (verificar cómo están registrados `IMovementService`/`IAccountService` y replicar).

## 3. Cambios frontend

### 3.1 Archivo nuevo
- `frontend/app/dashboard/page.tsx` (`"use client"`, patrón de `movimientos/page.tsx`).
  - Responsabilidad: fetch a `/api/dashboard` vía `apiFetch`; `401 → router.push("/login")`; estados carga / error / vacío; render responsive mobile-first.
  - Secciones (textos en español):
    1. "Saldo total" — una fila por moneda (`X.toFixed(2) MONEDA`), o mensaje vacío si no hay cuentas.
    2. "Ingresos del mes" y "Gastos del mes" — filas por moneda, totales en 0 si no hay movimientos.
    3. "Últimos movimientos" — lista de 5 con fecha, tipo (Ingreso/Gasto), monto + moneda, cuenta y categoría (reutilizar formato de tarjeta de `movimientos/page.tsx`).
    4. Mensaje de vacío ("No hay movimientos." / "Sin cuentas todavía") cuando corresponda.
    5. Links: "Ver historial" (`/movimientos`), "Ver cuentas" (`/cuentas`).

### 3.2 Archivos a modificar
- Ninguno obligatorio. Opcional (a criterio del implementador, no requerido por spec): agregar enlace a `/dashboard` desde la página principal o navbar si existe. No cambiar `movimientos/page.tsx` ni `lib/auth.ts`.

## 4. Flujo de datos

1. Usuario autenticado entra a `/dashboard` → `page.tsx` llama `apiFetch("/api/dashboard")` con `Bearer` (localStorage `finova_token`).
2. `DashboardController` (`[Authorize]`) extrae `userId` del claim `sub` → `IDashboardService.GetAsync(userId)`.
3. `DashboardService` ejecuta 3 queries EF Core filtradas por `userId` (nunca recibe IDs de otros usuarios), calcula con `decimal`, agrupa por `Currency` (string normalizado `ToUpperInvariant` ya en `AccountService`), ordena recientes, devuelve `DashboardResponse`.
4. Frontend renderiza por moneda sin convertir; `amount.toFixed(2)` solo para visualización.

Seguridad/aislamiento: todo el filtrado es `a.UserId == userId` / `m.Account.UserId == userId` en backend (constitución §1); frontend nunca decide qué datos mostrar. Sin endpoint por usuario → imposible pedir datos ajenos.

## 5. Decisiones técnicas y alternativas descartadas

| Decisión | Adoptada | Descartada y por qué |
|---|---|---|
| Endpoint | Nuevo `GET /api/dashboard` agregado en un solo round-trip | Reutilizar `GET /api/accounts` + `GET /api/movements/history` desde el frontend y sumar en cliente. Descartada: viola "cálculos en el backend" (constitución §2, spec RNF) y duplica lógica de período/moneda en el cliente. |
| Ubicación del cálculo de período | Backend (`DateTime.UtcNow`, mes calendario) | Calcular "día 1 → hoy" en el frontend y pasar `from/to`. Descartada: el período es regla de negocio fijada en spec; el cliente podría manipularlo. |
| Agrupación por moneda | `GroupBy(Currency)` en memoria tras queries EF, sumas `decimal` | Conversión entre monedas. Descartada: explícitamente fuera de spec (sin tasa de cambio definida). |
| Recientes | Top 5 en backend (`Take(5)`) | Traer todo el historial y cortar en frontend. Descartada: transfiere datos innecesarios. |
| `RunningBalance` en recientes | No incluir / ignorar (documentar) | Recalcular saldo resultante por fila como en history. Descartado: no es requisito del dashboard y añade costo/complejidad. |
| Zona horaria del "mes actual" | UTC (`DateTime.UtcNow`), consistente con `MovementService` que guarda UTC | Hora local del servidor o del cliente. Descartada: inconsistente con datos almacenados; genera desfases de día 1/fin de mes. Ver Decisión pendiente 1. |

## 6. Validaciones y casos de error

- Sin JWT / token inválido → `401` (framework, `[Authorize]`). Frontend redirige a `/login` (patrón existente).
- Usuario sin cuentas → `TotalBalances: []`, ingresos/gastos `[]`, `RecentMovements: []`; frontend muestra totales en 0 + mensaje de vacío (caso límite spec).
- Cuentas sin movimientos → aparecen en `TotalBalances` con 0 (vienen de `Accounts`, no de `Movements`).
- Cuentas en distintas monedas → una entrada por moneda; sin conversión.
- Movimientos futuros dentro del mes → incluidos (filtro por mes calendario, no por `Date <= hoy`).
- Montos: solo lectura agregada; no hay validación de entrada (sin body params). `decimal` en todo el cálculo, sin `float/double`.
- Consistencia: totales deben coincidir con `/api/movements/history` filtrado al mes + saldos de `/api/accounts` (criterio de aceptación).

## 7. Estrategia de pruebas

Backend (manual vía Swagger squeeze, sin suite de tests existente — verificar si hay proyecto de tests; si no, no crearlo en este ítem):
1. Con usuario A con 2 cuentas (ARS + USD) y movimientos de ingreso/gasto en mes actual y mes anterior → `GET /api/dashboard` devuelve saldos por moneda correctos; ingresos/gastos solo del mes actual; 5 recientes ordenados desc.
2. Movimiento futuro dentro del mes → incluido en totales.
3. Usuario B sin cuentas → arrays vacíos / ceros, `200` (no error).
4. Sin token → `401`. Con token de A → ningún dato de B (aislamiento: crear movimiento en B y confirmar que no aparece en A).
5. Cruzar con `GET /api/movements/history?from=<día1>` y `GET /api/accounts`: los montos coinciden.
6. Verificar JSON usa números con 2 decimales exactos (sin errores de float).

Frontend:
1. Login → `/dashboard` muestra las 3 secciones con datos correctos.
2. Usuario nuevo → mensajes de vacío, sin crash.
3. Sin token (limpiar localStorage) → redirige a `/login`.
4. Responsive: verificar en viewport móvil (375px) — tarjetas apiladas, sin scroll horizontal; textos en español.
5. Error de red/API caída → mensaje claro ("No se pudo cargar el panel."), no pantalla en blanco.
6. Confirmar que no se commitean secretos ni cambia formato de datos guardados.

## 8. Trazabilidad spec → plan

- "DEBE mostrar el saldo total agrupado por moneda" → `TotalBalances` (`DashboardService` paso 2) + sección "Saldo total" en `dashboard/page.tsx`.
- "DEBE mostrar el total de ingresos del período agrupado por moneda" → `TotalIncome` (paso 3, filtro mes calendario, `Type == Income`).
- "DEBE mostrar el total de gastos del período agrupado por moneda" → `TotalExpenses` (paso 3, `Type == Expense`).
- "DEBE mostrar los últimos N movimientos (N=5) con fecha, tipo, monto, cuenta y categoría" → `RecentMovements` top 5 + lista en frontend.
- "DEBE devolver 401 si no autenticado" → `[Authorize]` en `DashboardController` + redirect en frontend.
- "SOLO datos del usuario autenticado" → filtros `UserId` en las 3 queries; sin parámetros de usuario.
- RNF "cálculos en backend con decimal" → `DashboardService` con `decimal`; frontend solo formatea.
- RNF "español, responsive móvil" → textos en español, layout `Container maxWidth="sm"` + tarjetas como en historial.
- Casos límite (vacío / multimoneda / futuros) → §6.
- Fuera de alcance (gráficos, comparativas, filtros personalizados) → no se implementa; endpoint sin parámetros de fecha.

## 9. Decisiones pendientes

1. **Zona horaria del "mes actual"**: el plan asume UTC (consistente con almacenamiento). Si los usuarios están en otra zona (ej. Argentina UTC−3), un movimiento del día 1 local podría caer en el mes anterior UTC. Si el Coordinator quiere mes en hora local, definirlo antes de implementar (afecta `from` y el filtro de mes). No bloquea el resto del plan.
2. **`RunningBalance` en `RecentMovements`**: el plan propone reutilizar `MovementHistoryResponse` ignorando ese campo. Confirmar si se prefiere un DTO reciente sin ese campo para no confundir al consumidor.
3. **Enlace de navegación hacia `/dashboard`**: ¿debe la home o navbar enlazar al dashboard? No está en la spec; por defecto no se toca navegación existente.

## 10. Archivos afectados (resumen)

- Nuevos: `DTOs/DashboardResponse.cs`, `Interfaces/IDashboardService.cs`, `Services/DashboardService.cs`, `Controllers/DashboardController.cs`, `frontend/app/dashboard/page.tsx`.
- Modificar: `backend/Finova.API/Program.cs` (registro DI).
- No tocar: spec.md, entidades, DbContext (sin migración), `MovementService`, `AccountService`, `MovementsController`, historial frontend, secretos.
