# Spec — Dashboard (roadmap tarea 6)

## Contexto y objetivo
Dar al usuario una vista resumida de su situación financiera al entrar a la app: saldos, totales del período y últimos movimientos.

## Usuarios
- Usuario autenticado de Finova.

## Historias de usuario
- Como usuario, quiero ver mi saldo total para saber cuánto tengo.
- Como usuario, quiero ver el total de ingresos y gastos del período actual para controlar mis finanzas.
- Como usuario, quiero ver mis últimos movimientos para un acceso rápido a mi actividad reciente.

## Definiciones
- Saldo total: suma de los saldos de todas las cuentas del usuario, agrupada por moneda (no se convierten monedas).
- Período actual: mes calendario actual (desde el día 1 a hoy).
- Últimos movimientos: los N más recientes del usuario (orden por fecha descendente).

## Requisitos funcionales (EARS)
- El sistema DEBE mostrar el saldo total del usuario agrupado por moneda.
- El sistema DEBE mostrar el total de ingresos del período actual agrupado por moneda.
- El sistema DEBE mostrar el total de gastos del período actual agrupado por moneda.
- El sistema DEBE mostrar los últimos N movimientos con fecha, tipo, monto, cuenta y categoría.
- El sistema DEBE devolver 401 si el usuario no está autenticado.
- El sistema SOLO DEBE incluir datos del usuario autenticado.

## Requisitos no funcionales
- Cálculos en el backend con `decimal`.
- Respuesta en español, responsive con prioridad móvil.

## Casos límite
- Usuario sin cuentas o sin movimientos → totales en 0 y mensaje de vacío.
- Cuentas en distintas monedas → totales separados por moneda, sin conversión.
- Movimientos futuros dentro del mes → cuentan en los totales del período.

## Fuera de alcance
- Gráficos (V2).
- Comparaciones con meses anteriores (V2).
- Filtros de período personalizados (solo mes actual en V1).

## Criterios de finalización
- El dashboard muestra saldo total, ingresos, gastos del mes y últimos movimientos correctos.
- Los montos coinciden con el historial y los saldos de cuentas.
- Solo se ven datos del usuario autenticado.
- UI responsive con mensajes claros.

## Dudas abiertas
- (resueltas: período = mes calendario; totales agrupados por moneda sin conversión; últimos 5 movimientos; página dedicada `/dashboard`)

## Estado
- Spec aprobada, lista para planificación.
