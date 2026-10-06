# Tasks — Historial de movimientos

1. [x] Crear `MovementHistoryResponse` en Application.
2. [x] Agregar `GetHistoryAsync` a `IMovementService` e implementarlo en `MovementService` (saldo acumulado por cuenta + filtros).
3. [x] Agregar `GET /api/movements/history` en `MovementsController` con query params y manejo 400/401.
4. [x] Crear página `/movimientos` con filtros y lista (fecha, tipo, monto, cuenta, categoría, saldo resultante).
5. [x] Agregar botón "Historial" en el home.
6. [x] Validar: saldos correctos, filtros combinados, casos 400/401, vacío, responsive.
