# Tasks — Presupuestos

1. [x] Crear entidad `Budget` + Fluent API (índice único usuario+categoría+mes+moneda, FK Restrict a Category).
2. [x] Crear migración `AddBudgets` (verificar diff: solo CREATE TABLE) y aplicarla en Neon.
3. [x] Crear DTOs + `IBudgetService`/`BudgetService` (gastado del historial, estados 80/100, CRUD, PUT solo monto).
4. [x] Crear `BudgetsController` (GET ?year=&month=, POST, PUT, DELETE) con 400/404/401.
5. [x] Crear página `/presupuestos` (lista con progreso y chips, dialogs crear/editar, eliminar, errores en español).
6. [x] Validar: estados coinciden con historial, duplicado → 400, borrar categoría con presupuestos → 400, aislamiento, responsive.
