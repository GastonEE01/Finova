# Constitución — Finova

## Propósito

Finova es una aplicación web de gestión de finanzas personales.

El sistema permite a cada usuario gestionar sus cuentas, registrar ingresos y gastos, consultar su información financiera y, posteriormente, utilizar un asistente de IA basado en los datos reales de la aplicación.

## Principios innegociables

### 1. Aislamiento de datos

Cada usuario solo puede acceder, crear, modificar y eliminar sus propios datos.

Nunca se debe confiar únicamente en el frontend para garantizar el aislamiento. Las validaciones de autorización deben realizarse también en el backend.

### 2. Integridad financiera

Los cálculos financieros deben realizarse en el backend.

Los importes monetarios deben utilizar tipos adecuados para valores decimales y no `float` o `double`.

Los saldos deben derivarse de los movimientos correspondientes y mantenerse consistentes cuando los movimientos se creen, modifiquen o eliminen.

### 3. Seguridad

Las contraseñas nunca deben almacenarse en texto plano.

Las credenciales, connection strings, tokens y otros secretos nunca deben incluirse en el código fuente ni en el repositorio.

Las operaciones protegidas deben requerir autenticación y autorización.

### 4. Arquitectura

El backend debe respetar Clean Architecture y mantener separadas las responsabilidades de Domain, Application, Infrastructure y API.

El frontend debe mantener separada la presentación de la lógica de negocio.

Las decisiones de negocio deben permanecer en el backend y no depender exclusivamente del frontend.

### 5. Convenciones de código

El código, nombres de clases, entidades, propiedades, métodos, variables y tablas de base de datos deben utilizar nombres en inglés.

Los textos visibles para el usuario deben estar en español.

Se debe priorizar código simple, claro y mantenible.

### 6. Experiencia de usuario

La aplicación debe ser responsive y priorizar la experiencia en dispositivos móviles.

Las interfaces deben proporcionar mensajes claros cuando una operación sea exitosa o falle.

### 7. IA

El asistente de IA debe utilizar únicamente información proporcionada por el backend.

La IA no debe inventar datos financieros, saldos, movimientos, estadísticas ni resultados.

La IA no debe acceder a información perteneciente a otros usuarios.

Los cálculos financieros deben ser realizados por el backend antes de proporcionar los resultados necesarios a la IA.

### 8. Desarrollo guiado por especificaciones

Las nuevas funcionalidades deben comenzar con una especificación antes de su implementación.

La especificación debe definir:

* Requisitos.
* Límites.
* Casos de error.
* Criterios de aceptación.

Las especificaciones deben mantenerse actualizadas cuando cambie el comportamiento esperado del sistema.

### 9. Validación

Una funcionalidad no se considera terminada hasta comprobar que cumple su especificación.

Se deben verificar tanto los casos exitosos como los casos de error y los límites relevantes.

### 10. Cambios de arquitectura

Las decisiones que afecten significativamente la arquitectura, seguridad, modelo de datos o reglas de negocio deben ser identificadas antes de implementar la funcionalidad y, cuando corresponda, quedar documentadas.

## Flujo de desarrollo

Para nuevas funcionalidades se seguirá este flujo:

1. Especificación.
2. Clarificación.
3. Planificación.
4. Definición de tareas.
5. Implementación.
6. Validación.
7. Actualización de la especificación y documentación cuando sea necesario.

## Regla final

Ante cualquier conflicto entre una implementación rápida y los principios de esta constitución, se debe priorizar la seguridad, integridad de los datos, aislamiento entre usuarios y mantenibilidad del sistema.
