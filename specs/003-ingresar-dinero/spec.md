# Spec — Ingresar dinero (roadmap tarea 3)

## Requisitos
- El usuario puede registrar un ingreso indicando:
  - Monto (> 0, con hasta 2 decimales).
  - Fecha (puede ser pasada, actual o futura).
  - Cuenta (obligatoria, debe pertenecer al usuario autenticado).
  - Categoría (opcional, de tipo Income y del usuario autenticado).
  - Descripción (opcional).
- El ingreso se registra con `Type = Income`.
- El monto se interpreta en la moneda de la cuenta.
- El saldo de la cuenta aumenta en el monto del ingreso.
- Tras registrarlo, el usuario ve el nuevo saldo y el movimiento listado.

## Límites
- No se puede registrar un ingreso para una cuenta inexistente o de otro usuario.
- La categoría debe ser del usuario y de tipo Income (si se indica).
- El monto debe ser mayor a 0 y con hasta 2 decimales.
- La fecha debe ser una fecha válida.

## Casos de error
- Sin token o token inválido/vencido → 401.
- Cuenta inexistente o de otro usuario → 404.
- Categoría inexistente, de otro usuario o de tipo distinto de Income → 400.
- Monto ≤ 0, monto con más de 2 decimales, fecha inválida → 400.

## Criterios de aceptación
- `POST /api/movements` con `Type = Income` crea el movimiento y devuelve el movimiento creado.
- `GET /api/accounts/{id}` refleja el saldo + monto del ingreso.
- `GET /api/movements` (o el listado del usuario) incluye el movimiento con tipo "Ingreso", monto, fecha, categoría y descripción.
- El front muestra la página (ej. `/movimientos/nuevo`) con campos de monto, fecha, cuenta, categoría (opcional) y descripción, y tras registrar muestra mensaje de éxito y lista de movimientos.
- Se valida autorización: no se puede usar una cuenta o categoría de otro usuario.
