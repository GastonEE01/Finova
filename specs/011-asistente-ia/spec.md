# Spec — Asistente financiero con IA (roadmap V3)

## Contexto y objetivo
Permitir al usuario consultar en lenguaje natural su situación financiera ("¿cuánto gasté este mes?") con respuestas basadas únicamente en sus datos reales.

## Usuarios
- Usuario autenticado de Finova.

## Historias de usuario
- Como usuario, quiero preguntar "¿cuánto gasté este mes?" y recibir el total real de mis gastos.
- Como usuario, quiero preguntar "¿en qué gasté más?" y ver mis categorías top.
- Como usuario, quiero preguntar "¿cuánto tengo en total?" y ver mis saldos.
- Como usuario, quiero preguntar "¿gasté más que el mes pasado?" y ver la comparación.
- Como usuario, quiero pedir consejos ("¿cómo reduzco mis gastos?") basados en mis datos reales.

## Definiciones
- Snapshot financiero: resumen calculado por el backend (totales, top categorías, saldos, comparaciones) que se entrega al modelo como ÚNICA fuente de verdad.
- La IA NUNCA inventa cifras: si el dato no está en el snapshot, debe decir que no lo sabe.
- Todo aislamiento por usuario se aplica al construir el snapshot (solo datos propios).

## Requisitos funcionales (EARS)
- El sistema DEBE responder las 5 preguntas ejemplo con datos reales del usuario.
- El sistema DEBE calcular todos los montos en el backend antes de pedir la respuesta al modelo.
- El sistema DEBE rechazar (401) consultas sin autenticación.
- El sistema DEBE registrar/incluir la clave del proveedor solo fuera del repo (user-secrets/variables).
- El sistema DEBE responder en español.

## Requisitos no funcionales
- Latencia razonable (< 15s por respuesta); timeout con mensaje claro si el proveedor falla.
- Sin secretos en código ni repo; sin datos de otros usuarios en el prompt.

## Casos límite
- Usuario sin movimientos → el modelo responde que aún no hay datos (no inventa).
- Proveedor caído/sin crédito → 503 con mensaje "Asistente no disponible, intentá más tarde".
- Pregunta fuera de finanzas personales → el modelo la deriva a temas financieros o dice que no puede ayudar.

## Fuera de alcance
- Historial de conversación persistente (solo última pregunta/respuesta en V1).
- Acciones desde el chat (crear movimientos por texto).
- Voz o archivos adjuntos.

## Criterios de finalización
- Las 5 preguntas responden cifras que coinciden con historial/dashboard.
- Con usuario sin datos no inventa. Sin token → 401. Sin key del proveedor → error claro de configuración.

## Dudas abiertas
- (resueltas: motor Ollama local; alcance las 5 preguntas; UI página /asistente; sin datos avisa sin inventar)

## Estado
- Spec aprobada, lista para planificación.
