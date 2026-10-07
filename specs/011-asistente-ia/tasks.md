# Tasks — Asistente IA

1. [x] Crear DTOs (`AssistantDtos.cs`: request con pregunta, response con respuesta).
2. [x] Crear `IAssistantService` + `AssistantService` (snapshot financiero + llamada a Ollama + system prompt anti-invención).
3. [x] Crear `AssistantController` (`POST /api/assistant/chat`, [Authorize], 400/401/503).
4. [x] Registrar HttpClient + servicio en `Program.cs`; sección `Ollama` en appsettings.
5. [x] Crear página `/asistente` (chat stateless, 401→login, estados carga/error) + enlace en home.
6. [x] Verificar: builds OK; las 5 preguntas responden cifras del dashboard; sin datos no inventa; Ollama caído → 503; aislamiento; responsive.
