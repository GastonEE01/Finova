# Spec — Presupuestos (roadmap V2 tarea 3)

## Contexto y objetivo
Permitir al usuario definir cuánto quiere gastar por categoría en el mes y ver su avance, con avisos al acercarse o superar el límite.

## Usuarios
- Usuario autenticado de Finova.

## Historias de usuario
- Como usuario, quiero definir un monto límite de gasto por categoría para el mes.
- Como usuario, quiero ver cuánto llevo gastado de cada presupuesto.
- Como usuario, quiero que me avisen cuando me acerco o supero el límite.

## Definiciones
- Presupuesto: monto límite para gastos de una categoría en un mes específico (año+mes).
- Gastado: suma de gastos de esa categoría en ese mes (moneda de las cuentas; presupuestos por moneda).
- Estado: OK (<80%), Acercándose (≥80%), Superado (≥100%).

## Requisitos funcionales (EARS)
- El sistema DEBE permitir crear un presupuesto (categoría de gasto predefinida del sistema o propia, monto > 0, mes, moneda).
- El sistema DEBE mostrar cada presupuesto con gastado, restante y % de avance.
- El sistema DEBE indicar el estado (OK / acercándose / superado).
- El sistema DEBE permitir editar y eliminar presupuestos propios.
- El sistema DEBE devolver 401 sin autenticación y 404 ante presupuesto ajeno/inexistente.
- El sistema SOLO DEBE incluir datos del usuario autenticado.

## Requisitos no funcionales
- Cálculos en backend con `decimal`; UI en español, responsive; nueva migración (tablas nuevas).

## Casos límite
- Categoría inexistente, de tipo Income o personalizada de otro usuario → 400.
- Presupuesto duplicado (misma categoría+mes+moneda) → 400.
- Monto ≤ 0 → 400.
- Mes anterior en 0 no aplica (sin variación aquí).

## Fuera de alcance
- Presupuestos recurrentes automáticos (se crean por mes).
- Umbrales configurables por presupuesto (fijos 80/100).
- Alertas push/email (solo visual en UI).

## Criterios de finalización
- CRUD funciona con estados correctos que coinciden con el historial.
- Avisos visibles al acercarse/superar. Solo datos propios. Responsive.

## Dudas abiertas
- (resueltas: presupuesto por mes específico; umbrales fijos 80/100)

## Estado
- Spec aprobada, lista para planificación.
