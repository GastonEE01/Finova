---

description: Divide un plan aprobado en tareas concretas de implementación
agent: plan
-----------

Quiero crear las tareas de implementación para:

$ARGUMENTS

Lee:

1. `AGENTS.md`
2. `MEMORY.md`
3. `docs/constitution.md`
4. `spec.md`
5. `plan.md`

Crea o actualiza:

`specs/NNN-nombre/tasks.md`

Cada tarea debe:

* Estar en orden de dependencia.
* Ser concreta y verificable.
* Tener relación con uno o más requisitos funcionales.
* Ser suficientemente pequeña para completarse en aproximadamente 20-30 minutos cuando sea razonable.
* Indicar claramente cuándo se considera terminada.

No implementes código.

Si aparecen más de 10 tareas, evalúa si la funcionalidad debería dividirse en varias specs.

No avances automáticamente a implementación.
