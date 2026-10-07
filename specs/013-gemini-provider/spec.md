# Spec — Proveedor Gemini para el asistente (V3.1)

## Contexto y objetivo
El asistente funciona con Ollama local, que no existe en producción. Agregar Google Gemini como proveedor en la nube, conmutable por entorno, manteniendo Ollama para desarrollo.

## Usuarios
- Usuario autenticado de Finova (sin cambios visibles; mismas 5 preguntas).

## Historias de usuario
- Como usuario, quiero que el asistente funcione también en producción, con la misma calidad que en local.
- Como desarrollador, quiero seguir usando Ollama gratis en local sin pagar nada.

## Definiciones
- Proveedor activo se elige por configuración (`AiProvider`: "Ollama" o "Gemini").
- La API key de Gemini vive SOLO en user-secrets (local) y App Settings (Azure). Nunca en archivos versionados.
- El snapshot, el prompt anti-invención y los códigos 400/401/503 no cambian.

## Requisitos funcionales (EARS)
- El sistema DEBE responder vía Gemini cuando `AiProvider=Gemini`, con los mismos datos del snapshot.
- El sistema DEBE seguir respondiendo vía Ollama cuando `AiProvider=Ollama` (default local).
- El sistema DEBE devolver 503 "Asistente no disponible" si Gemini falla o no hay key configurada.
- El sistema DEBE enviar solo el snapshot (totales, sin movimientos crudos ni datos de otros usuarios).
- El sistema DEBE responder en español sin inventar cifras (mismo system prompt).

## Requisitos no funcionales
- Timeout ~100s; temperature baja; sin SDK nuevo si basta HttpClient REST.
- Key fuera del repo (verificación obligatoria pre-commit).

## Casos límite
- Sin key con provider Gemini → 503 con mensaje claro (no 500).
- Cuota gratuita agotada (429) → 503 "no disponible, intentá más tarde".
- Respuesta vacía/malformada → 503.

## Fuera de alcance
- Streaming; historial persistente; cambio de modelo por usuario.

## Criterios de finalización
- Las 5 preguntas responden cifras del dashboard vía Gemini; 503 sin key; Ollama local intacto; sin secretos en repo.

## Dudas abiertas
- (resueltas: modelo gemini-3.8-flash; proveedor por config `AiProvider` — Ollama por defecto en local, Gemini en Azure)

## Estado
- Spec aprobada, lista para planificación.
