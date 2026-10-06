---

description: Crea el plan técnico de una spec aprobada
agent: plan
-----------

Quiero crear el plan técnico para:

$ARGUMENTS

Antes de hacerlo:

1. Lee `AGENTS.md`.
2. Lee `MEMORY.md`.
3. Lee `docs/constitution.md`.
4. Lee la `spec.md` completa.
5. Verifica que la spec esté aprobada y no tenga dudas abiertas importantes.
6. Analiza el código existente relacionado con la funcionalidad.

Crea o actualiza:

`specs/NNN-nombre/plan.md`

El plan debe incluir:

* Archivos que se modificarán.
* Archivos nuevos necesarios.
* Responsabilidad de cada cambio.
* Flujo de datos.
* Algoritmos o pseudocódigo cuando sea útil.
* Cambios de frontend y backend.
* Decisiones técnicas y alternativas descartadas.
* Estrategia de pruebas.
* Relación entre cada parte del plan y los requisitos funcionales de la spec.

No implementes código.

No cambies la spec para adaptar el plan. Si el plan revela una decisión que falta, detente y pregúntame.
