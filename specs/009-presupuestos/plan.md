# Plan técnico — 009 Presupuestos (V2.3)

Spec: `specs/009-presupuestos/spec.md` (APROBADA, no modificar). Este plan no cambia la spec.

## 1. Resumen
CRUD de presupuestos mensuales por categoría de gasto + cálculo de avance en backend (gastado, restante, % y estado con umbrales fijos 80/100). Página `/presupuestos` con MUI, español, responsive, progress bars. Nueva entidad + migración EF Core aditiva.

## 2. Entidades y migración
### 2.1 Nueva entidad `Budget` (Domain, nombres en inglés)
- `backend/Finova.Domain/Entities/Budget.cs` (NUEVO):
  - `Guid Id`, `Guid UserId`, `Guid CategoryId`, `int Year`, `int Month` (1-12), `decimal Amount` (límite), `string Currency` (max 3, ej. ARS).
  - Navegación: `User User`, `Category Category`.
- Validaciones de dominio (en servicio, no solo DataAnnotations): `Amount > 0`; `Month 1-12`, `Year` razonable (ej. 2000-2100); `Currency` requerida; `Category` debe existir, pertenecer al usuario y ser `Type == Expense` (regla AGENTS: presupuestos solo gastos).
- Unicidad: misma `UserId + CategoryId + Year + Month + Currency` → 400 duplicado. Implementar con índice único en Fluent API + chequeo previo en servicio para mensaje claro en español.

### 2.2 Fluent API en `FinovaDbContext`
- `DbSet<Budget> Budgets`.
- Config: `Amount HasPrecision(18,2)`; `Currency IsRequired MaxLength(3)`; índice único compuesto `(UserId, CategoryId, Year, Month, Currency)`; `Budget → User` Cascade; `Budget → Category` Restrict/NoAction (no borrar categoría con presupuestos: devolver 400/409 según decisión del implementador; NO cascade para no perder historial de presupuestos silenciosamente). Índice en `(UserId, Year, Month)` para listado mensual.
- `User` necesita colección `List<Budget>`; `Category` colección `List<Budget>` (opcional pero consistente con patrón actual).

### 2.3 Migración EF Core (aditiva, sin tocar tablas existentes)
- `dotnet ef migrations add AddBudgets --project Finova.Infrastructure --startup-project Finova.API` → solo `CREATE TABLE Budgets` + índices. Verificar que el diff no altere Users/Accounts/Categories/Movements. Aplicar sobre Neon (hay InitialCreate ya aplicada). Riesgo bajo; rollback = `Remove` migración si no aplicada.

## 3. Archivos a crear / modificar
**Crear:**
- `backend/Finova.Domain/Entities/Budget.cs`
- `backend/Finova.Application/DTOs/BudgetDtos.cs` (Create/Update/Response con `Spent, Remaining, Percent, Status`)
- `backend/Finova.Application/Interfaces/IBudgetService.cs`
- `backend/Finova.Infrastructure/Services/BudgetService.cs`
- `backend/Finova.API/Controllers/BudgetsController.cs`
- `frontend/app/presupuestos/page.tsx`
**Modificar:**
- `backend/Finova.Infrastructure/Persistence/FinovaDbContext.cs` (+ DbSet y Fluent API)
- `backend/Finova.Domain/Entities/User.cs`, `Category.cs` (+ colecciones navegación)
- `backend/Finova.Infrastructure/Migrations/` (nueva migración)
- `backend/Finova.API/Program.cs` (registro DI `IBudgetService → BudgetService`, mismo patrón que Account/Movement)
- `frontend/app/lib/auth.ts` — no cambiar; reutilizar `apiFetch`.
- Opcional: link en home/dashboard hacia `/presupuestos`.

## 4. Endpoints y DTOs
Base `api/budgets`, `[Authorize] + GetUserId()` (patrón MovementsController).
- `GET /api/budgets?year=&month=` — lista propia del mes (si no se pasa, mes actual UTC). Cada ítem: `id, categoryId, categoryName, year, month, amount, currency, spent, remaining, percent, status` (`Ok|Acercandose|Superado`).
- `POST /api/budgets` — body `{ categoryId, year, month, amount, currency }`. Errores: 401 sin auth; 400 categoría Income/ajena, monto ≤0, mes inválido, duplicado; 404 categoría inexistente (o 400 según convención actual de MovementService que usa 400 para categoría — mantener 400 para coherencia).
- `PUT /api/budgets/{id}` — edita `amount` (y opcionalmente mes/categoría/moneda; si cambia clave única, revalidar duplicado). Ajeno/inexistente → 404.
- `DELETE /api/budgets/{id}` — propio. Ajeno/inexistente → 404.
- DTOs en inglés, mensajes de error `{ mensaje }` en español (patrón MovementsController).

## 5. Flujo de datos y cálculo (backend)
- `BudgetService`:
  - Crear/actualizar: valida auth-aislamiento (`Category.UserId == userId`, `Budget.UserId == userId`), monto, mes, duplicado.
  - Listar: trae presupuestos del usuario del mes; para cada uno calcula `Spent = SUM(Movements WHERE Account.UserId==userId AND CategoryId==budget.CategoryId AND Type==Expense AND Date UTC en [año,mes] AND Account.Currency==budget.Currency)`. `Remaining = Amount - Spent`; `Percent = Spent/Amount*100` (decimal, redondeo a 2); `Status = Percent>=100 Superado, >=80 Acercandose, else Ok`.
  - Nota: `Spent` se deriva del historial (coherente con spec "coinciden con el historial"). Sin tabla de alertas; el estado se computa al leer.
- Concurrencia/duplicado: chequeo `AnyAsync` + índice único como resguardo (capturar `DbUpdateException` → 400).

## 6. Frontend `/presupuestos`
- Reutilizar patrón `cuentas/page.tsx`: `apiFetch`, redirect a `/login` en 401, `Dialog` crear/editar, lista con `LinearProgress` de MUI (determinate, value min(percent,100)), color: success/warning/error según estado; chip de estado en español ("OK", "Acercándose", "Superado"); texto `gastado / límite · restante X`; filtros año/mes (TextField numérico o DatePicker de mes simple); eliminar con confirmación; mensajes de error del backend (`mensaje`).
- Responsive: `Container maxWidth="sm"`, columna en móvil.
- Moneda: campo texto 3 letras (patrón cuentas) + selector de categoría (solo gastos del usuario, GET categorías existentes).

## 7. Decisiones y alternativas
- Entidad separada `Budget` vs. campo en Category: se elige entidad (spec exige mes específico + moneda + CRUD independiente).
- `Spent` calculado al leer vs. columna persistida: se elige calculado (siempre consistente con movimientos; evita triggers/recalcular en edición de movimientos).
- FK Category Restrict vs. Cascade: Restrict (evita borrar presupuestos al borrar categoría). **Decisión pendiente:** mensaje/código exacto al intentar borrar categoría con presupuestos (400 con mensaje vs. 409) — definir en tasks/implementación sin cambiar spec.
- Umbrales fijos 80/100 en servicio (constantes), no configurables (fuera de alcance).

## 8. Seguridad y validaciones
- `[Authorize]` en controller; todo filtrado por `userId` del token; 404 ante ajeno (no revelar existencia). Validar moneda coincide con cuentas al calcular (gastos en otra moneda no suman). `decimal` en todo cálculo.

## 9. Estrategia de pruebas
- Backend build + endpoints con Swagger/Thunder: crear presupuesto válido; duplicado → 400; categoría income → 400; monto 0 → 400; editar; eliminar; GET con gastos previos verifica `spent/percent/status` vs. `/movements/history`; aislamiento (otro usuario → 404); 401 sin token.
- Frontend build; responsive móvil; progress bar refleja estado; textos en español.
- Migración: `dotnet ef migrations script` inspeccionado (solo CREATE Budgets); aplicar en dev/Neon; verificar tablas existentes intactas.

## 10. Trazabilidad con la spec
- Crear (categoría gasto, monto>0, mes, moneda) → POST + validaciones §4/5.
- Mostrar gastado/restante/% → GET con cálculo §5.
- Estado OK/acercándose/superado → umbrales §5, chip+barra §6.
- Editar/eliminar propios → PUT/DELETE §4.
- 401/404 y solo datos propios → §8.
- Fuera de alcance (recurrentes, umbrales configurables, push/email) → no se implementa.

## 11. Riesgos / decisiones pendientes
- **Decisión resuelta por el usuario:** bloquear con 400 (primero eliminar sus presupuestos).
- **Decisión resuelta por el usuario:** PUT solo edita el monto (categoría, mes y moneda inmutables).
- Riesgo: presupuestos en moneda sin movimientos → `spent=0`, estado OK (aceptado).
