# Spec — Historial de movimientos (roadmap tarea 5)

## Contexto y objetivo
Permitir que cada usuario consulte el historial completo de sus ingresos y gastos, con filtros, para entender sus finanzas.

## Usuarios
- Usuario autenticado de Finova.

## Historias de usuario
- Como usuario, quiero ver la lista de todos mis movimientos (ingresos y gastos) ordenados por fecha para revisar mi actividad.
- Como usuario, quiero filtrar por fecha, cuenta o categoría para encontrar movimientos específicos.
- Como usuario, quiero ver en cada movimiento su fecha, monto, tipo, categoría, cuenta y saldo resultante.

## Definiciones
- Saldo resultante: saldo de la cuenta inmediatamente después de ese movimiento (ingresos suman, gastos restan, en la moneda de la cuenta).
- Los filtros se combinan entre sí (operan como AND).

## Requisitos funcionales (EARS)
- El sistema DEBE listar todos los movimientos del usuario ordenados por fecha descendente.
- Cada fila DEBE mostrar: fecha, monto, tipo (Ingreso/Gasto), categoría (o "Sin categoría"), cuenta y saldo resultante.
- El sistema DEBE permitir filtrar por rango de fechas (desde/hasta).
- El sistema DEBE permitir filtrar por cuenta (solo cuentas del usuario).
- El sistema DEBE permitir filtrar por categoría (solo categorías del usuario).
- El sistema DEBE devolver 401 si el usuario no está autenticado.
- El sistema SOLO DEBE devolver movimientos de cuentas del usuario autenticado.

## Requisitos no funcionales
- Respuesta en español, responsive con prioridad móvil.
- Montos con `decimal`; saldos derivados de movimientos en el backend.

## Casos límite
- Sin movimientos → lista vacía con mensaje ("No hay movimientos").
- Filtros sin resultados → mismo mensaje de vacío.
- Fecha desde posterior a fecha hasta → 400 con mensaje claro.
- Cuenta/categoría de otro usuario en el filtro → se ignoran o 404 (definir).

## Fuera de alcance
- Editar/eliminar movimientos.
- Paginación (V1 trae todo; evaluar si crece).
- Gráficos y resúmenes (tareas 6+ / V2).
- Exportar (CSV/PDF).

## Criterios de finalización
- El historial muestra fecha, monto, tipo, categoría, cuenta y saldo resultante correctos.
- Los filtros por fecha, cuenta y categoría funcionan combinados.
- Solo se ven datos del usuario autenticado.
- UI responsive con mensajes claros de éxito, error y vacío.

## Dudas abiertas
- (resueltas: saldo resultante siempre por fila calculado en el back; filtro por tipo incluido; página dedicada `/movimientos`; filtro ajeno devuelve 200 con lista vacía)

## Estado
- Spec aprobada, lista para planificación.
