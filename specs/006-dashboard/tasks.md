# Tasks — Dashboard

1. [x] Crear `DTOs/DashboardResponse.cs` (saldos por moneda, ingresos/gastos del mes por moneda, últimos 5 movimientos).
2. [x] Crear `IDashboardService` + `DashboardService` (mes calendario en UTC, `decimal`, aislamiento por usuario).
3. [x] Crear `DashboardController` con `GET /api/dashboard` ([Authorize]).
4. [x] Registrar `IDashboardService` en `Program.cs`.
5. [x] Crear página `/dashboard` (saldos, mes actual, últimos 5, estados vacíos, responsive).
6. [x] Agregar botón "Dashboard" en el home.
7. [x] Verificar: montos coinciden con historial/cuentas, 401 sin token, vacío, responsive.
