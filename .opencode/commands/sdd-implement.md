---

description: Implementa las tareas aprobadas de una spec de Finova
agent: build
------------

Quiero implementar:

$ARGUMENTS

Antes de modificar código:

1. Lee `AGENTS.md`.
2. Lee `MEMORY.md`.
3. Lee `docs/constitution.md`.
4. Lee `spec.md`, `plan.md` y `tasks.md`.
5. Verifica que la spec, el plan y las tareas estén aprobados.
6. Analiza el código existente y reutiliza estructuras cuando corresponda.

Implementa las tareas en orden de dependencia.

Reglas:

* La spec tiene prioridad sobre el plan si existe una contradicción.
* No agregues funcionalidades que no estén en la spec.
* Respeta la arquitectura existente.
* Respeta las reglas de seguridad y aislamiento de usuarios.
* Valida los casos de error relevantes.
* No cambies requisitos sin actualizar primero la spec.
* Mantén `tasks.md` actualizado con las tareas completadas.
* Actualiza `MEMORY.md` cuando corresponda según las reglas del proyecto.

Al finalizar cada bloque importante, explica qué se implementó y qué se verificó.
