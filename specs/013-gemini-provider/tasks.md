# Tasks — Proveedor Gemini

1. [x] Crear `GeminiChatClient : IChatClient` (REST generateContent, temperature 0.2, 429/sin key/vacía → 503).
2. [x] Crear `AiChatClientFactory` por config `AiProvider` (default Ollama) + DI en `Program.cs`.
3. [x] Agregar sección `AiProvider`/`Gemini` con placeholders en appsettings.json (sin secretos).
4. [x] Ajuste mínimo en `AskAssistantUseCase` según plan + tests del cliente Gemini (HttpClient mockeado).
5. [x] Verificar: build, tests, Ollama intacto, grep de no-secretos en repo.
   - `dotnet test Finova.Tests`: 78/78 en verde (69 previos + 9 nuevos).
   - Smoke Ollama: POST /api/assistant/chat (usuario nuevo) → "Aún no tenés datos registrados." + 401 sin token.
   - `grep AIza|npg_` en backend+s specs: vacío. Sin key real usada ni guardada.
   - Pendiente (requiere reiniciar la API y key del usuario): E2E con `AiProvider=Gemini` y 503 sin key.
