# AGENTS.md — Finova
Descripción: aplicación web de gestión de finanzas personales que permite registrar ingresos, gastos y cuentas, visualizar la situación financiera y consultar un asistente de IA que analiza los datos reales del usuario.

## Stack y estructura
- Next.js para el front,MUI para usar componentes hechos,reactIcon y para el back .NET

## Convenciones
- Textos de la interfaz en español.
- Código simple, nombres descriptivos y comentarios solo donde aporten.
- Diseño limpio y responsive; cualquier pantalla nueva debe verse bien en el móvil
- En el back usar la arquitectura limpia por capas: API,Application,Domain y Infrastruture

## Reglas de dominio / trampas conocidas
- Cada usuario solo puede acceder y modificar sus propias cuentas y movimientos.
- Un movimiento debe pertenecer obligatoriamente a una cuenta del usuario que lo creó.
- Cada movimiento debe ser de tipo ingreso o gasto.
- Los montos deben ser mayores a 0.
- La moneda debe estar definida para cada cuenta.
- Los movimientos de una cuenta deben utilizar la misma moneda que la cuenta.
- Las cuentas pueden tener saldo positivo, cero o negativo, según el tipo de cuenta.
- El saldo de una cuenta se calcula a partir de sus movimientos y no debe modificarse manualmente sin una operación de dominio válida.
- Eliminar un movimiento debe actualizar correctamente el saldo de la cuenta.
- Editar un movimiento debe recalcular correctamente el saldo de la cuenta.
- Las fechas de los movimientos no pueden ser inválidas.
- Un gasto reduce el saldo de la cuenta.
- Un ingreso aumenta el saldo de la cuenta.
- Existen categorías predefinidas del sistema, visibles para todos los usuarios. Las categorías personalizadas pertenecen al usuario que las creó y no deben poder utilizarse entre usuarios. La creación de categorías personalizadas queda para una funcionalidad futura.
- En los gastos, la categoría es opcional (puede quedar "Sin categoría"); la descripción sigue siendo obligatoria.
- Los presupuestos se aplican únicamente a gastos de la categoría correspondiente.
- La IA nunca debe inventar información financiera. Las respuestas relacionadas con los datos del usuario deben basarse en información obtenida de la aplicación.
- La IA no puede acceder directamente a datos de otros usuarios.
- Los cálculos financieros deben realizarse en el backend y la IA debe recibir los resultados necesarios para generar la respuesta.
- Los valores monetarios deben manejarse con precisión decimal y no mediante operaciones de punto flotante que puedan producir errores de redondeo.

## Forma de trabajar
- Antes de implementar una nueva funcionalidad, analizar primero la especificación correspondiente.
- Si no existe una especificación para una nueva funcionalidad, crearla antes de modificar código.
Analizar el código existente y reutilizar las estructuras actuales cuando sea apropiado.
- Antes de realizar cambios importantes de arquitectura, seguridad, modelo de datos o reglas de negocio, explicar el impacto y solicitar aprobación.
- Al terminar una funcionalidad, explicar qué se modificó y cómo fue validada.
Mantener MEMORY.md actualizado cuando cambien decisiones, arquitectura, funcionalidades completadas o próximos pasos relevantes.

## Límites
- ⚠️ Pregunta antes: crear archivos nuevos, cambiar el formato de los datos guardados.
- No modificar ni eliminar datos financieros de otros usuarios.
- No incluir secretos reales en archivos versionados.
- No introducir cambios de arquitectura importantes sin explicarlos previamente.
- No modificar el comportamiento definido en una especificación sin actualizar - primero la especificación correspondiente.

## Verificación
- Verificar los casos exitosos.
- Verificar los casos de error relevantes.
- Verificar las reglas de autorización y aislamiento entre usuarios cuando corresponda.
- Verificar que los cálculos financieros sean correctos cuando corresponda.
- Verificar que el frontend siga siendo responsive.
- Comprobar que no se hayan introducido secretos en los archivos versionados.
- Confirmar que la implementación cumple la especificación activa.

## Spec-Driven Development
- Las nuevas funcionalidades deben comenzar con una especificación antes de implementar código.
- Leer docs/constitution.md antes de trabajar en una nueva funcionalidad.
- Cuando exista una spec activa en specs/, leer su spec.md, plan.md y tasks.md antes de modificar código relacionado.
- La especificación debe definir requisitos, límites, casos de error y criterios de aceptación.
- Las especificaciones deben mantenerse actualizadas cuando cambie el comportamiento esperado del sistema.
- No implementar una nueva funcionalidad hasta que su plan haya sido revisado y aprobado.
- La implementación debe validarse contra los requisitos y criterios de aceptación de la especificación.

## Git
Realizar commits con mensajes descriptivos.
No subir secretos, credenciales, archivos generados ni dependencias instaladas al repositorio.
Antes de hacer push, verificar los archivos incluidos en el commit.