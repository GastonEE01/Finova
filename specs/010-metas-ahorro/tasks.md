# Tasks — Metas de ahorro

1. [x] Crear entidad `SavingGoal` + `GoalId` nullable en `Movement` + Fluent API (SetNull).
2. [x] Crear migración `AddSavingGoalsWithContributionLink` (verificar diff) y aplicarla en Neon.
3. [x] Crear DTOs + `ISavingGoalService`/`SavingGoalService` (progreso derivado, estados, CRUD, contribuciones como gastos vinculados).
4. [x] Crear `SavingGoalsController` (`api/savinggoals`: GET/POST/PUT/DELETE + POST `/{id}/contributions`) con 400/404/401.
5. [x] Crear página `/metas` (lista con progreso/restante/chips, dialogs crear/editar/aportar con cuenta+categoría+monto).
6. [x] Validar: aporte descuenta cuenta y aparece en historial, excedente, vencida, eliminar desvincula sin alterar saldos, aislamiento, responsive.
