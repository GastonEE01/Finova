# Spec — Registrar gastos (roadmap tarea 4)

## Contexto y objetivo
Permitir que cada usuario registre sus gastos asociados a una cuenta, disminuyendo su saldo.

## Usuarios
- Usuario autenticado de Finova.

## Historias de usuario
- Como usuario, quiero registrar un gasto indicando monto, fecha, cuenta, descripción y, opcionalmente, una categoría para reflejar mis salidas de dinero.
- Como usuario, quiero ver el gasto registrado en el listado de movimientos.

## Definiciones
- Gasto: movimiento de tipo Expense.
- En gastos, la categoría es opcional (puede quedar "Sin categoría").
- En gastos, la descripción es obligatoria.
- El saldo de una cuenta disminuye por el monto de cada gasto.

## Requisitos funcionales (EARS)
- El sistema DEBE permitir registrar un gasto con: monto (> 0), fecha (pasada, actual o futura), cuenta (obligatoria), categoría (opcional; si se indica debe ser tipo Expense y predefinida del sistema o propia) y descripción (obligatoria).
- El sistema DEBE guardar el gasto con `Type = Expense` y el monto en la moneda de la cuenta.
- El sistema DEBE disminuir el saldo de la cuenta en el monto del gasto.
- El sistema DEBE devolver el gasto creado con 201.
- El sistema DEBE listar los gastos del usuario junto a los ingresos, ordenados por fecha descendente.
- El sistema DEBE devolver 401 si el usuario no está autenticado.
- El sistema DEBE devolver 404 si la cuenta no existe o no pertenece al usuario.
- El sistema DEBE devolver 400 si el monto es ≤ 0, la fecha es inválida, la categoría no es válida o la descripción está vacía.

## Requisitos no funcionales
- Los montos se manejan con `decimal` (no float/double).
- El saldo se calcula derivado de los movimientos, nunca editable a mano.
- La interfaz debe ser responsive y en español.

## Casos límite
- Gasto sin categoría → permitido (queda "Sin categoría").
- Gasto con categoría inexistente, personalizada de otro usuario o de tipo Income → 400.
- Gasto en cuenta ajena → 404.
- Monto con más de 2 decimales → redondear o rechazar (definir 400 con mensaje claro).

## Fuera de alcance
- Editar o eliminar movimientos.
- Filtros por fecha/cuenta/categoría en el historial (tarea 5).
- Presupuestos (V2).

## Criterios de finalización
- El gasto se crea correctamente y disminuye el saldo de la cuenta.
- El listado de movimientos muestra el gasto con tipo, monto, fecha, categoría (o "Sin categoría") y descripción.
- Los casos de error devuelven los códigos esperados.
- La UI es usable en móvil y muestra mensajes claros de éxito y error.

## Dudas abiertas
- (ninguna pendiente)
