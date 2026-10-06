# Spec — Comparaciones (roadmap V2 tarea 2)

## Contexto y objetivo
Permitir al usuario comparar su comportamiento financiero: mes actual vs. anterior, dónde gasta más y cómo evolucionan sus gastos.

## Usuarios
- Usuario autenticado de Finova.

## Historias de usuario
- Como usuario, quiero comparar mis ingresos y gastos de este mes vs. el mes anterior para saber si mejoré o empeoré.
- Como usuario, quiero ver las categorías donde más gasto para detectar fugas de dinero.
- Como usuario, quiero ver la evolución de mis gastos para entender su tendencia.

## Definiciones
- Variación mensual: diferencia porcentual entre el total del mes actual y el del mes anterior (por tipo y moneda).
- Top categorías: categorías con mayor gasto acumulado en el mes actual, con monto y porcentaje del total.
- Evolución de gastos: total de gastos por mes en los últimos 6 meses.
- Todo en una sola moneda por consulta (mismo patrón que Gráficos, con selector).

## Requisitos funcionales (EARS)
- El sistema DEBE mostrar ingresos y gastos del mes actual vs. mes anterior con variación % por moneda.
- El sistema DEBE mostrar el top 5 de categorías por gasto del mes actual con monto y % del total.
- El sistema DEBE mostrar la evolución mensual de gastos de los últimos 6 meses.
- El sistema DEBE devolver 401 si el usuario no está autenticado.
- El sistema SOLO DEBE incluir datos del usuario autenticado.

## Requisitos no funcionales
- Cálculos en el backend con `decimal`; división por cero controlada (mes anterior en 0 → variación nula/"—" en vez de error).
- UI en español, responsive, reutilizando MUI X Charts y el patrón de secciones del dashboard.

## Casos límite
- Mes anterior sin movimientos → variación no calculable (mostrar "—").
- Sin gastos en el mes → top vacío con mensaje.
- Menos de 6 meses de historia → meses faltantes en 0.

## Fuera de alcance
- Presupuestos y metas (tareas V2.3/2.4).
- Comparación por cuenta individual (solo moneda).
- Períodos personalizados.

## Criterios de finalización
- Las comparaciones coinciden con el historial filtrado por mes.
- Los porcentajes son correctos (spot-check manual).
- Solo datos del usuario autenticado; UI responsive con vacíos y errores.

## Datos de prueba
- No se insertan datos de prueba: se verifica con los datos existentes del usuario (decisión del usuario).

## Dudas abiertas
- (resueltas: top 5; evolución mensual 6 meses; sección en `/dashboard`; sin seed de datos)

## Estado
- Spec aprobada, lista para planificación.
