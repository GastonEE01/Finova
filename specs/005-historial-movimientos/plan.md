# Plan — Historial de movimientos

## Cambios en backend
- `Application/DTOs/MovementHistoryResponse.cs` (nuevo): `MovementResponse` + `AccountName`, `Currency`, `RunningBalance`.
- `Application/Interfaces/IMovementService.cs`: agregar `GetHistoryAsync(userId, from, to, accountId, categoryId, type)`.
- `Infrastructure/Services/MovementService.cs`: trae movimientos del usuario ordenados por fecha+Id, calcula saldo acumulado por cuenta sobre TODOS los movimientos (para que el saldo resultante sea correcto aunque haya filtros), y filtra en memoria por fecha/cuenta/categoría/tipo. Cuenta o categoría ajena → lista vacía.
- `API/Controllers/MovementsController.cs`: `GET /api/movements/history?from&to&accountId&categoryId&type` → 200. `from > to` → 400.
- Se mantiene `GET /api/movements` sin cambios (lo usan las páginas de alta).

## Cambios en frontend
- Nueva página `/movimientos`: filtros (desde/hasta, cuenta, categoría, tipo) + lista con fecha, tipo, monto+moneda, cuenta, categoría y saldo resultante.
- Home: botón "Historial".

## Flujo de datos
`/movimientos` → `GET /history` con query params → servicio calcula saldos acumulados → lista desc por fecha.

## Estrategia de pruebas
- Historial con ingresos+gastos muestra saldos resultantes correctos por cuenta.
- Filtros combinados y cada uno por separado.
- Filtro ajeno → 200 vacío. `from > to` → 400. Sin token → 401.
- Responsive móvil.

## Relación con requisitos
- Cada requisito funcional mapea a `GetHistoryAsync` + página `/movimientos`.
