# Spec — Gráficos (roadmap V2 tarea 1)

## Contexto y objetivo
Dar al usuario visualizaciones de sus finanzas: en qué gasta, cómo se comparan ingresos y gastos, y cómo evoluciona su saldo.

## Usuarios
- Usuario autenticado de Finova.

## Historias de usuario
- Como usuario, quiero ver mis gastos por categoría para saber en qué gasto más.
- Como usuario, quiero comparar ingresos vs. gastos por mes para ver mi balance.
- Como usuario, quiero ver la evolución de mi saldo para entender su tendencia.

## Definiciones
- Gastos por categoría: suma de gastos agrupados por categoría en el período.
- Ingresos vs. gastos: totales por mes para los últimos N meses.
- Evolución del saldo: saldo acumulado día por día en el período.
- Todos los gráficos usan una sola moneda a la vez (sin conversión).

## Requisitos funcionales (EARS)
- El sistema DEBE mostrar los gastos por categoría del período (gráfico de torta o barras).
- El sistema DEBE mostrar ingresos vs. gastos de los últimos 6 meses (barras agrupadas).
- El sistema DEBE mostrar la evolución diaria del saldo del mes (línea).
- El sistema DEBE permitir elegir la moneda cuando el usuario tenga más de una.
- El sistema DEBE devolver 401 si el usuario no está autenticado.
- El sistema SOLO DEBE incluir datos del usuario autenticado.

## Requisitos no funcionales
- Cálculos agregados en el backend con `decimal`.
- Gráficos responsive (móvil primero), textos en español.
- Nueva dependencia de gráficos en el frontend (a aprobar).

## Casos límite
- Sin movimientos en el período → gráfico vacío con mensaje.
- Una sola moneda → sin selector.
- Categorías sin nombre (gasto sin categoría) → agrupar como "Sin categoría" solo donde aplique.

## Fuera de alcance
- Comparaciones mes vs. mes anterior con porcentajes (tarea V2.2).
- Presupuestos, metas (tareas V2.3/2.4).
- Exportar gráficos.
- Filtros por cuenta individual en gráficos (solo moneda en V1).

## Criterios de finalización
- Los 3 gráficos muestran datos correctos que coinciden con el historial.
- Solo se ven datos del usuario autenticado.
- UI responsive con mensajes claros de vacío y error.

## Dudas abiertas
- (resueltas: períodos fijos — gastos del mes, comparativa 6 meses, evolución diaria del mes; selector de moneda; gráficos como secciones de `/dashboard`; librería MUI X Charts)

## Estado
- Spec aprobada, lista para planificación.
