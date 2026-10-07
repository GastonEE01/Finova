# Plan — 013-gemini-provider (Proveedor Gemini para el asistente)

## 1. Objetivo
Agregar Google Gemini (`gemini-3.8-flash`) como proveedor de IA en producción, conmutable por configuración (`AiProvider`: `Ollama` | `Gemini`), sin cambiar snapshot, system prompt anti-invención ni códigos HTTP (400/401/503). Ollama sigue como default local. Sin SDK nuevo: HttpClient REST directo.

## 2. Arquitectura actual (reutilizar)
- `IChatClient.GetAnswerAsync(systemPrompt, question)` — abstracción única; `AskAssistantUseCase` depende solo de ella.
- `OllamaChatClient` (Infrastructure/Repositories): lee `Ollama:BaseUrl/Model`, POST `/api/chat`, temperature 0.2, parsea `message.content`.
- `AskAssistantUseCase`: construye snapshot (decimal/UTC/moneda principal), system prompt anti-invención, mapea `HttpRequestException/TaskCanceledException` + respuesta vacía → `AssistantUnavailableException` (mensaje actual dice "Ollama no disponible").
- `AssistantController POST /api/assistant/chat`: 400 pregunta vacía/>500, 503 `Asistente no disponible…`, `[Authorize]`.
- DI actual: `AddHttpClient<IChatClient, OllamaChatClient>(timeout 100s)`.

## 3. Diseño propuesto
### 3.1 Nuevo cliente Gemini (único archivo nuevo de código)
- Crear `backend/Finova.Infrastructure/Repositories/GeminiChatClient.cs : IChatClient`.
- Config: `Gemini:ApiKey` (SOLO user-secrets local / App Settings Azure, nunca en repo), `Gemini:Model` = `gemini-3.8-flash`.
- Sin key (vacía) → lanzar `AssistantUnavailableException` directamente (no llamar a red; el UseCase ya lo convierte en 503).
- Request REST: `POST https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}` con body:
  ```json
  {
    "system_instruction": { "parts": [{ "text": "<systemPrompt>" }] },
    "contents": [{ "role": "user", "parts": [{ "text": "<question>" }] }],
    "generationConfig": { "temperature": 0.2 }
  }
  ```
- Parseo respuesta: `candidates[0].content.parts[].text` concatenados y `Trim()`. Si no hay texto → retornar `string.Empty` (el UseCase lo convierte en 503 por "respuesta vacía").
- 429 o cualquier `!IsSuccessStatusCode` → `HttpRequestException` (el UseCase lo convierte en 503). No loguear la key.
- Respetar `HttpClient.Timeout` de 100s provisto por DI (no setear otro timeout interno).

### 3.2 Selector por configuración (factory, sin cambiar UseCase ni Controller)
- Crear `backend/Finova.Infrastructure/Repositories/AiChatClientFactory.cs : IChatClient` que delega según `AiProvider`:
  - `AiProvider` == `Gemini` (case-insensitive) → delega a `GeminiChatClient`.
  - Cualquier otro valor / ausente → delega a `OllamaChatClient` (default local).
- Ambas implementaciones concretas se registran como Transients tipados; la factory es la que se expone como `IChatClient`.
- Alternativa descartada: `if` en `Program.cs` eligiendo una sola implementación. Se prefiere factory para no reiniciar decisiones de DI por entorno y mantener ambos clientes testeables.

### 3.3 DI (`Program.cs`) — modificar
- Reemplazar `AddHttpClient<IChatClient, OllamaChatClient>` por:
  - `AddHttpClient<OllamaChatClient>(timeout 100s)` + `AddHttpClient<GeminiChatClient>(timeout 100s)` + `AddScoped<IChatClient, AiChatClientFactory>`.
- Sin otros cambios de DI. Frontend sin cambios.

### 3.4 Configuración — modificar (solo placeholders, jamás secretos)
- `backend/Finova.API/appsettings.json`: agregar
  ```json
  "AiProvider": "Ollama",
  "Gemini": { "Model": "gemini-3.8-flash" }
  ```
  (NO incluir `ApiKey` en el archivo versionado.)
- Secretos: local `dotnet user-secrets set "Gemini:ApiKey" "<key>"`; Azure App Settings `Gemini:ApiKey` + `AiProvider=Gemini`.
- Documentar en el plan/tasks el comando, no el valor.

### 3.5 Ajuste menor en `AskAssistantUseCase` — modificar (1 línea)
- Generalizar mensaje de excepción interna `"Ollama no disponible"` → `"Proveedor IA no disponible"` (el mensaje público 503 del controller NO cambia). Evita filtrar el proveedor activo y mantiene trazabilidad. Resto del UseCase (snapshot, prompt, validaciones) intacto.

## 4. Archivos a crear / modificar
| Acción | Archivo |
|---|---|
| CREAR | `backend/Finova.Infrastructure/Repositories/GeminiChatClient.cs` |
| CREAR | `backend/Finova.Infrastructure/Repositories/AiChatClientFactory.cs` |
| MODIFICAR | `backend/Finova.API/Program.cs` (DI §3.3) |
| MODIFICAR | `backend/Finova.API/appsettings.json` (placeholders §3.4) |
| MODIFICAR | `backend/Finova.Application/UseCases/AskAssistantUseCase.cs` (mensaje genérico §3.5) |
| CREAR (tests) | `backend/Finova.Tests/GeminiChatClientTests.cs` (ver §6) |
| NO TOCAR | `AssistantController.cs`, snapshot, prompt, frontend, migraciones |

## 5. Seguridad y aislamiento
- Key SOLO en user-secrets / App Settings; verificación pre-commit (`git status` + `grep -ri "gemini.*key\|AIza" --include="*.json" --include="*.cs"` debe dar vacío salvo placeholders).
- Solo se envía el snapshot (totales, sin movimientos crudos); sin datos de otros usuarios (el snapshot ya se construye por `userId`).
- Cálculos en backend; IA solo recibe snapshot + pregunta (constitución §7, AGENTS.md).
- Respuesta en español, anti-invención: mismo system prompt, temperature 0.2.

## 6. Estrategia de pruebas
### Unitarias (xUnit+Moq, nuevo `GeminiChatClientTests.cs`, con HttpMessageHandler mockeado)
1. Parsea `candidates[0].content.parts[*].text` y concatena correctamente.
2. Respuesta vacía/malformada (`{}` sin candidates) → retorna vacío (→ 503 en UseCase).
3. HTTP 429 → lanza `HttpRequestException` (→ 503 en UseCase).
4. Sin `Gemini:ApiKey` → lanza `AssistantUnavailableException` sin llamada HTTP.
5. Factory: `AiProvider=Gemini` delega a Gemini; ausente/`Ollama`/valor inválido delega a Ollama.
- Existentes: los 3 tests de `AssistantAndCategoryUseCasesTests` deben seguir en verde sin cambios (mockean `IChatClient`).

### Smoke manual (criterios de la spec)
1. `AiProvider=Ollama` (local, sin key): las 5 preguntas responden con cifras del dashboard (Ollama intacto).
2. `AiProvider=Gemini` + key válida (user-secrets): las mismas 5 preguntas responden cifras del dashboard vía Gemini (comparar contra GET /api/dashboard).
3. `AiProvider=Gemini` sin key → 503 `{ mensaje: "Asistente no disponible, intentá más tarde" }` (no 500).
4. 400 pregunta vacía/larga y 401 sin token: sin cambios.
5. `grep` de no-secretos + `git status` limpio de secretos antes del commit.
6. `dotnet build` solución 0 errores + `dotnet test` en verde.

## 7. Trazabilidad (spec → plan)
- R1 responde vía Gemini con mismo snapshot → §3.1 + §3.2.
- R2 Ollama default local → factory default §3.2 + `AiProvider: Ollama` en appsettings.
- R3 503 si falla/sin key → sin-key §3.1, 429/vacía §3.1 + UseCase existente.
- R4 solo snapshot → sin cambios en snapshot; §5.
- R5 español sin invención, mismo prompt → prompt intacto, temperature 0.2 en `generationConfig`.
- No funcionales (timeout 100s, HttpClient sin SDK, key fuera del repo) → §3.1/§3.4/§5.
- Fuera de alcance (streaming, historial persistente, modelo por usuario) → no incluido.

## 8. Riesgos
- Cuota gratuita Gemini (429) en validación → mitigado: mapeo a 503 ya definido.
- Modelo `gemini-3.8-flash` deprecado a futuro → mitigado: `Gemini:Model` por config, cambio sin código.
- Key accidental en repo → mitigado: verificación pre-commit obligatoria (§6.5).

## 9. Decisiones pendientes
- Ninguna: modelo, selector por config, almacenamiento de key, endpoint REST y mapeo 503 ya aprobados en la spec. Si durante la implementación la forma exacta de `system_instruction`/`generationConfig` del endpoint v1beta difiriera, adaptar el DTO manteniendo el contrato `IChatClient` y reportarlo en el resumen (cambio menor de cableado, no de diseño).
