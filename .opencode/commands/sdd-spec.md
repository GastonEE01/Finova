---

description: Crea una especificación SDD para una nueva funcionalidad de Finova
agent: plan
-----------

Quiero crear una especificación SDD para:

$ARGUMENTS

Antes de escribir la especificación:

1. Lee `AGENTS.md`.
2. Lee `MEMORY.md`.
3. Lee `docs/constitution.md`.
4. Analiza la estructura actual del proyecto y las funcionalidades relacionadas.
5. Revisa las specs existentes para evitar duplicaciones o contradicciones.
6. Identifica las decisiones que todavía necesiten aclaración.

Crea la especificación en:

`specs/NNN-nombre/spec.md`

La spec debe definir:

* Contexto y objetivo.
* Usuarios.
* Historias de usuario.
* Definiciones necesarias.
* Requisitos funcionales verificables utilizando EARS.
* Requisitos no funcionales.
* Casos límite.
* Fuera de alcance.
* Criterios de finalización.
* Dudas abiertas.

La spec debe describir QUÉ y POR QUÉ, no CÓMO implementarlo.

No incluyas arquitectura, nombres de archivos, clases, endpoints ni código.

No modifiques código.

Si falta una decisión importante, detente y pregúntame antes de marcar la spec como aprobada.
