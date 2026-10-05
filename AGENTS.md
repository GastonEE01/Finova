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
- Las categorías de gastos e ingresos pertenecen al usuario y no deben poder utilizarse entre usuarios.
- Los presupuestos se aplican únicamente a gastos de la categoría correspondiente.
- La IA nunca debe inventar información financiera. Las respuestas relacionadas con los datos del usuario deben basarse en información obtenida de la aplicación.
- La IA no puede acceder directamente a datos de otros usuarios.
- Los cálculos financieros deben realizarse en el backend y la IA debe recibir los resultados necesarios para generar la respuesta.
- Los valores monetarios deben manejarse con precisión decimal y no mediante operaciones de punto flotante que puedan producir errores de redondeo.

## Forma de trabajar
- Cuándo planificar antes de tocar código, tamaño de los cambios, qué explicar al
terminar.

## Límites
- ✅ Siempre: respetar las reglas de fechas y racha, mantener los textos en español.
- ⚠️ Pregunta antes: crear archivos nuevos, cambiar el formato de los datos guardados.

## Verificación
- Cómo comprobar que un cambio funciona antes de darlo por terminado.