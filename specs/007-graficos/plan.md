# Plan técnico — 007 Gráficos (roadmap V2 tarea 1)

## 1. Objetivo y alcance
Agregar 3 visualizaciones a la página existente `frontend/app/dashboard/page.tsx`, con datos agregados calculados en el backend:
1. Gastos por categoría del mes actual (torta o barras).
2. Ingresos vs. gastos de los últimos 6 meses (barras agrupadas).
3. Evolución diaria del saldo del mes actual (línea).

Decisiones aprobadas que este plan respeta (no reabrir):
- Períodos fijos: mes actual (gastos por categoría + evolución diaria) y últimos 6 meses calendario (comparativa).
- Selector de moneda en el front; cada gráfico usa una sola moneda, sin conversión.
- Los gráficos son secciones nuevas de `/dashboard` (no crear ruta `/graficos`).
- Librería: MUI X Charts, nueva dependencia `@mui/x-charts`.

## 2. Estado actual reutilizable
- `backend/Finova.Infrastructure/Services/DashboardService.cs`: patrón de agregación (mes calendario UTC con `DateTime.UtcNow`, filtros por `Account.UserId`, `decimal`, `MovementType.Income/Expense`, moneda vía `Account.Currency`).
- `backend/Finova.API/Controllers/DashboardController.cs`: patrón `[Authorize]` + `GetUserId()` desde claims `sub`/`NameIdentifier`.
- `backend/Finova.Application/DTOs/DashboardResponse.cs`: DTOs `CurrencyTotal`, `DashboardResponse` (serialización camelCase ya usada por el front).
- `frontend/app/dashboard/page.tsx`: estructura actual (fetch con `apiFetch`, redirect a `/login` en 401, estados carga/error/vacío, textos en español, `Container maxWidth="sm"`).
- Registro DI en `backend/Finova.API/Program.cs` (`IDashboardService` → `DashboardService`).

## 3. Diseño de API (backend)

### 3.1. Opción elegida: 3 endpoints nuevos bajo `DashboardController`, con query `?currency=`
Se proponen endpoints separados (uno por gráfico) en lugar de extender `GET /api/dashboard`, porque:
- Cada gráfico filtra por una sola moneda; el dashboard actual devuelve todas las monedas.
- Evitan sobrecargar el payload del dashboard y permiten carga/error independientes por gráfico en el front.
- Reutilizan el mismo controller, autorización y `GetUserId()`.

| Endpoint | Parámetros | Respuesta |
|---|---|---|
| `GET /api/dashboard/expenses-by-category?currency=ARS` | `currency` requerido (código de moneda de cuenta, ej. `ARS`) | `{ currency, periodFrom, periodTo, items: [{ categoryId?, categoryName, total }] }` |
| `GET /api/dashboard/income-vs-expenses?currency=ARS` | `currency` requerido | `{ currency, months: [{ year, month, label, income, expense }] }` (6 meses calendario hasta el actual, en orden cronológico) |
| `GET /api/dashboard/balance-evolution?currency=ARS` | `currency` requerido | `{ currency, periodFrom, periodTo, points: [{ date, balance }] }` (un punto por día del mes actual, día 1 → hoy UTC) |

Reglas comunes:
- `[Authorize]`; 401 si no autenticado (automático por el esquema JWT existente).
- Aislamiento: todos los queries filtran por `m.Account.UserId == userId` (nunca por `AccountId` suelto).
- Filtro de moneda: `m.Account.Currency == currency` (comparación exacta; normalizar a mayúsculas en backend).
- `currency` ausente o vacío → `400` con mensaje en español ("Debe indicar la moneda.").
- Moneda sin cuentas del usuario → respuesta `200` con colección vacía (el front muestra mensaje de vacío, no error).
- Todos los montos `decimal`; serialización numérica existente.
- Períodos en UTC, coherente con `DashboardService` (mes calendario UTC). Documentar en el DTO que el corte es UTC.

### 3.2. Agregaciones (lógica en `DashboardService` o nuevo servicio)
Extender `IDashboardService` + `DashboardService` (recomendado: mismo servicio, 3 métodos nuevos; alternativa descartada: crear `ChartsService` separado — innecesario, duplica inyección y el dominio es el mismo "dashboard").

1. **ExpensesByCategory(currency, userId)**:
   - Base: movimientos `Type == Expense`, `Account.UserId == userId`, `Account.Currency == currency`, `Date` en mes actual UTC (`Year == now.Year && Month == now.Month`, mismo criterio que el dashboard actual).
   - Agrupar por `CategoryId`/`Category.Name`; `Category == null` → `categoryId: null`, `categoryName: "Sin categoría"`.
   - `total = Sum(Amount)` por grupo, orden descendente por total.
2. **IncomeVsExpenses(currency, userId)**:
   - Ventana: 6 meses calendario hacia atrás incluyendo el actual (ej. si hoy es octubre → mayo..octubre). Calcular `start = primer día del mes (now - 5 meses)`.
   - Traer movimientos de la ventana filtrados por usuario+moneda; agrupar por `(Year, Month)` y `Type`; el backend rellena los meses sin movimientos con `0` (el front no debe inferir meses faltantes).
   - `label` en español generado en el front (ej. "may", "jun") o devuelto por el backend como `"2026-05"`; recomendado: backend devuelve `year/month` y el front formatea la etiqueta en español.
3. **BalanceEvolution(currency, userId)**:
   - Puntos diarios día 1 → hoy (UTC) del mes actual.
   - Criterio de saldo: **saldo acumulado de movimientos** de las cuentas del usuario en esa moneda con `Date <= día` (ingreso suma, gasto resta). Nota: esto incluye movimientos de meses anteriores (saldo real acumulado), no solo movimientos del mes. Ver §7 "Decisión pendiente" si se quisiera solo variación del mes.
   - Implementación: una sola query de movimientos (`Date <= hoy`, usuario+moneda, solo `Date/Type/Amount`) y acumulación día por día en memoria; volumen acotado por usuario, aceptable. Ordenar por `Date`.
   - Punto inicial: día 1 con saldo acumulado hasta ese día (no forzar 0).

### 3.3. DTOs nuevos (`backend/Finova.Application/DTOs/`, archivo nuevo `DashboardChartsResponse.cs`)
Nombres en inglés (constitución §5), propiedades en inglés:
- `ExpensesByCategoryResponse { string Currency; DateTime PeriodFrom; DateTime PeriodTo; List<CategoryExpenseItem> Items; }` + `CategoryExpenseItem { Guid? CategoryId; string CategoryName; decimal Total; }`
- `IncomeVsExpensesResponse { string Currency; List<MonthlyTotalItem> Months; }` + `MonthlyTotalItem { int Year; int Month; decimal Income; decimal Expense; }`
- `BalanceEvolutionResponse { string Currency; DateTime PeriodFrom; DateTime PeriodTo; List<BalancePointItem> Points; }` + `BalancePointItem { DateTime Date; decimal Balance; }`

### 3.4. Archivos backend a crear/modificar
| Archivo | Cambio |
|---|---|
| `backend/Finova.Application/DTOs/DashboardChartsResponse.cs` | CREAR: DTOs del §3.3 |
| `backend/Finova.Application/Interfaces/IDashboardService.cs` | MODIFICAR: agregar 3 firmas (`GetExpensesByCategoryAsync`, `GetIncomeVsExpensesAsync`, `GetBalanceEvolutionAsync(userId, currency)`) |
| `backend/Finova.Infrastructure/Services/DashboardService.cs` | MODIFICAR: implementar los 3 métodos según §3.2 |
| `backend/Finova.API/Controllers/DashboardController.cs` | MODIFICAR: agregar 3 actions `[HttpGet("expenses-by-category")]`, `[HttpGet("income-vs-expenses")]`, `[HttpGet("balance-evolution")]` con `[FromQuery] string currency`, validación de vacío → 400, llamada al servicio con `GetUserId()` |

Sin cambios de modelo ni migraciones (solo lectura sobre `Movements`/`Accounts`).

## 4. Diseño frontend

### 4.1. Dependencia
- Agregar `@mui/x-charts` a `frontend/package.json` (única dependencia nueva aprobada por la spec).
- **Decisión pendiente**: verificar al instalar la versión de `@mui/x-charts` compatible con `react 19.2.8`, `MUI/material ^9.4.0` y `next 16.3.8`; fijar la versión mayor exacta que declare soporte (al redactar este plan, la línea v8 de MUI X declara soporte de React 19; confirmar en la documentación/npm al momento de instalar). Instalar con `npm install @mui/x-charts` y correr `npm run build` para validar.

### 4.2. Componentes y estructura (en `frontend/app/dashboard/page.tsx`, sin nueva ruta)
- `page.tsx` (MODIFICAR): mantener las secciones actuales (saldos, totales, últimos movimientos) y agregar debajo una sección "Gráficos":
  - Selector de moneda: `Select` de MUI con las monedas derivadas de `data.totalBalances` (+ ingresos/gastos). Si hay una sola moneda → fijarla sin mostrar selector (spec: "Una sola moneda → sin selector"). Si no hay monedas (sin cuentas) → mensaje de vacío y no pedir gráficos.
  - Estado `currency` seleccionado (por defecto la primera moneda disponible); al cambiar, re-fetchear los 3 endpoints con `?currency=`.
  - Tres componentes hijos (CREAR archivos para no engordar `page.tsx`):
    - `frontend/app/dashboard/expenses-by-category-chart.tsx` → `PieChart` (torta; alternativa: `BarChart` si hay muchas categorías — dejar torta por defecto; si >8 categorías, el implementador puede usar barras, documentarlo).
    - `frontend/app/dashboard/income-vs-expenses-chart.tsx` → `BarChart` con dos series (Ingresos, Gastos), eje x = etiquetas de mes en español.
    - `frontend/app/dashboard/balance-evolution-chart.tsx` → `LineChart`, eje x = días del mes.
  - Cada componente recibe `currency` por props, hace su propio `apiFetch`, y maneja estados propios: carga ("Cargando gráfico..."), vacío ("Sin movimientos en el período." / "Sin datos para {moneda}."), error ("No se pudo cargar el gráfico."). Textos en español.
  - 401 en cualquier fetch → `router.push("/login")` (mismo patrón actual).
- Layout responsive móvil-primero: cada gráfico en `Card`/`Box` a ancho completo, altura fija (~250–300px), `Container` actual `maxWidth="sm"` es suficiente; verificar scroll horizontal inexistente en 360px.
- Formato de montos: `toFixed(2)` + código de moneda (patrón actual); etiquetas de ejes cortas.

### 4.3. Archivos frontend a crear/modificar
| Archivo | Cambio |
|---|---|
| `frontend/package.json` | MODIFICAR: agregar `@mui/x-charts` |
| `frontend/app/dashboard/page.tsx` | MODIFICAR: sección "Gráficos" + selector de moneda + composición de los 3 componentes |
| `frontend/app/dashboard/expenses-by-category-chart.tsx` | CREAR: torta de gastos por categoría del mes |
| `frontend/app/dashboard/income-vs-expenses-chart.tsx` | CREAR: barras agrupadas 6 meses |
| `frontend/app/dashboard/balance-evolution-chart.tsx` | CREAR: línea de evolución diaria del saldo |

## 5. Flujo de datos (ejemplo)
1. Usuario autenticado abre `/dashboard` → `GET /api/dashboard` (existente) → front deriva monedas disponibles.
2. Front fija `currency=ARS` → en paralelo:
   - `GET /api/dashboard/expenses-by-category?currency=ARS`
   - `GET /api/dashboard/income-vs-expenses?currency=ARS`
   - `GET /api/dashboard/balance-evolution?currency=ARS`
   (cada una con `Authorization: Bearer <token>` vía `apiFetch`).
3. Backend: `[Authorize]` → `GetUserId()` → `DashboardService` agrega con `decimal` filtrando `Account.UserId + Currency + período UTC` → DTOs JSON.
4. Front renderiza MUI X Charts; al cambiar moneda se repite el paso 2.

## 6. Decisiones técnicas y alternativas descartadas
| Decisión | Elegido | Descartado y por qué |
|---|---|---|
| Granularidad de endpoints | 3 endpoints nuevos con `?currency=` | Extender `GET /api/dashboard`: mezcla multimoneda con series por moneda, payload pesado y rompe el contrato actual del front |
| Dónde agregar | Extender `DashboardService`/`DashboardController` | Nuevo `ChartsController/Service`: fragmenta un mismo agregado de lectura; más archivos sin beneficio |
| Períodos | Fijos (mes actual / 6 meses / diario del mes) | Parámetros de fecha arbitrarios: fuera de alcance de la spec V1 |
| Moneda | Una por consulta, sin conversión | Conversión multimoneda: requiere tasas de cambio, explícitamente fuera de alcance |
| Librería | MUI X Charts | Recharts/Tremor/Chart.js: MUI X integra tema y tipado con MUI 9 ya usado; decisión ya aprobada por el usuario |
| Etiquetas de mes | Backend devuelve `year/month`, front formatea en español | Backend devuelve strings localizados: mezcla presentación en API |
| Meses vacíos en comparativa | Backend rellena con 0 | Front rellena: duplica lógica de calendario y riesgo de meses faltantes |

## 7. Decisiones pendientes (no asumir en implementación)
1. **Versión exacta de `@mui/x-charts`** compatible con React 19.2 + MUI 9 + Next 16.3.8 al momento de instalar (fijar y validar con `npm run build`).
2. **Definición de "saldo" en evolución diaria**: APROBADO por el usuario — saldo real acumulado con arrastre de meses anteriores (punto inicial día 1 con saldo acumulado hasta ese día, no 0).
3. **Tipo del gráfico de categorías** si hay muchas (>8): torta por defecto; se acepta cambiar a barras en ese caso.

## 8. Seguridad, validaciones y errores
- Autorización: `[Authorize]` en controller; 401 automático sin token (requisito spec). Front redirige a `/login`.
- Aislamiento: todos los queries con `Account.UserId == userId`; verificar con pruebas que un usuario no ve datos de otro (moneda ajena → vacío, no error revelador).
- Validación: `currency` requerido → 400 si vacío; normalizar mayúsculas; la moneda la elige el front entre las propias del usuario.
- Integridad financiera: sumas con `decimal` en backend; el front solo formatea, nunca calcula totales.
- Casos límite (spec): sin movimientos → `200` con listas vacías + mensaje en UI; "Sin categoría" solo en gastos por categoría; una sola moneda → sin selector.
- Sin cambios de escritura: endpoints de solo lectura, no afectan saldos.

## 9. Estrategia de pruebas y validación
- **Backend** (`dotnet build` + pruebas manuales con Swagger/Bearer):
  - Usuario con movimientos en 2 monedas: cada `?currency=` devuelve solo su moneda y coincide con `/movimientos` (historial) filtrado igual.
  - Gastos por categoría: suma por categoría = suma de gastos del mes en historial; gasto sin categoría aparece como "Sin categoría".
  - Comparativa: 6 entradas ordenadas, meses sin movimientos en 0; ingresos/gastos coinciden con historial por mes.
  - Evolución: puntos día 1→hoy, valores acumulados coherentes (spot-check contra historial).
  - Aislamiento: segundo usuario no ve datos del primero; sin token → 401; sin `currency` → 400.
- **Frontend** (`npm run build`, prueba manual desktop + móvil 360px):
  - Las 3 secciones cargan con datos correctos; cambio de moneda recarga los 3 gráficos.
  - Una sola moneda → sin selector; sin movimientos → mensajes de vacío; error de red → mensaje de error; sesión expirada → redirect a login.
  - Responsive: sin scroll horizontal, gráficos legibles en móvil.
- **Criterios de aceptación** (trazabilidad con spec): §9-caso1 → pruebas de coincidencia con historial; §9-caso2 → pruebas de aislamiento; §9-caso3 → pruebas responsive/vacío/error.

## 10. Orden de implementación sugerido (para tasks.md)
1. Backend: DTOs → interfaz → servicio → controller → build + verificación Swagger.
2. Frontend: instalar `@mui/x-charts` (resolver pendiente #1) → selector de moneda en `page.tsx` → 3 componentes → build + verificación responsive.
3. Validación cruzada contra historial y criterios de la spec.
