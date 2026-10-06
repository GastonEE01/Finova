---

description: Valida una implementación contra su spec de Finova
agent: build
------------

Quiero validar la implementación de:

$ARGUMENTS

Lee:

1. `AGENTS.md`
2. `MEMORY.md`
3. `docs/constitution.md`
4. `spec.md`
5. `plan.md`
6. `tasks.md`
7. El código implementado.

Verifica cada requisito funcional y cada criterio de finalización de la spec.

Comprueba cuando corresponda:

* Casos exitosos.
* Casos de error.
* Autorización y aislamiento entre usuarios.
* Integridad de los datos.
* Cálculos financieros.
* Frontend responsive.
* Compatibilidad con la arquitectura existente.

Para pruebas que puedan automatizarse, utiliza las herramientas de testing apropiadas para el stack real de Finova.

No asumas `node --test` si no corresponde al código que se está validando.

Si algún requisito no está cubierto o falla, indícalo claramente.

Al finalizar, entrega un veredicto:

* Spec cumplida.
* Spec parcialmente cumplida.
* Spec no cumplida.

No marques una spec como cumplida si existe algún criterio de finalización importante que no haya sido verificado.
