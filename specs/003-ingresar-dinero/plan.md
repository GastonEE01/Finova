# Plan — Ingresar dinero

## Backend
- `Application/DTOs/MovementResponse.cs` (Id, AccountId, CategoryId, Type, Amount, Date, Description, CategoryName).
- `Application/DTOs/CreateMovementRequest.cs` (Amount, Date, AccountId, CategoryId?, Type, Description?).
- `Application/Interfaces/IMovementService.cs` → `CreateAsync`, `GetAllByUserAsync`.
- `Infrastructure/Services/MovementService.cs`:
  - Validar cuenta del usuario (404 si no).
  - Validar categoría del usuario y tipo Income cuando aplique (400).
  - Validar monto > 0 y fecha válida (400).
  - Guardar `Movement` con `Type = Income`.
- `API/Controllers/MovementsController.cs` (`[Authorize]`):
  - `POST /api/movements` → 201 con el movimiento.
  - `GET /api/movements` → lista del usuario (orden por fecha desc).

## Frontend
- Página `/movimientos/nuevo`:
  - Select de cuenta (desde `GET /api/accounts`).
  - Select de categoría opcional (Income, del usuario — requiere `GET /api/categories`; si no existe, crear endpoint simple).
  - Inputs de monto y fecha (date picker).
  - Descripción opcional.
  - Botón registrar → éxito, muestra lista actualizada de movimientos.

## Verificación
- Alta de ingreso sube el saldo de la cuenta (check vía `GET /api/accounts/{id}`).
- Lista de movimientos muestra el ingreso.
- Errores (401, 404, 400) manejados en la UI con mensajes en español.
- Responsive en móvil.
