# Plan técnico — 011 Asistente financiero con IA (Ollama local)

## 1. Objetivo y alcance
Implementar chat financiero en español (`/asistente`) que responde las 5 preguntas de la spec usando SOLO datos reales calculados por el backend. Motor: Ollama local vía HTTP (`http://localhost:11434`). Stateless (sin historial persistente). Sin modificar spec.md.

## 2. Arquitectura y flujo de datos
```
Front /asistente (historial en pantalla, input, enviar, apiFetch)
  → POST /api/assistant/chat { question: string } [Authorize]
    → AssistantController: GetUserId() + validación (vacía → 400)
    → IAssistantService.GetAnswerAsync(userId, question)
        1. Construye snapshot (solo datos del userId, decimal, UTC) reutilizando lógica de DashboardService.
        2. Arma system prompt (español, "solo snapshot, si no está → decir no lo sé, sin inventar, derivar temas no financieros").
        3. Llama a Ollama vía HttpClient: POST {BaseUrl}/api/chat { model, messages:[system,user], stream:false } con timeout 60–120s.
        4. Devuelve { answer }.
    → Front muestra burbujas usuario/asistente.
Errores: sin token → 401 (middleware JWT + front redirige a /login); Ollama caído/timeout → 503 "Asistente no disponible, intentá más tarde"; usuario sin movimientos → snapshot con totales 0 y flag HasData=false → prompt obliga a responder "aún no hay datos".
```

## 3. Forma del snapshot (DTO interno, no exponer movimientos crudos)
```csharp
AssistantSnapshot {
  bool HasData;
  List<CurrencyTotal> TotalBalances;      // saldo por moneda (suma ingresos−gastos por cuenta)
  List<CurrencyTotal> MonthIncome;        // mes calendario UTC actual por moneda
  List<CurrencyTotal> MonthExpenses;      // idem gastos
  MonthTotalItem CurrentMonth; MonthTotalItem PreviousMonth;  // por moneda principal (la de mayor saldo; si empate, primera)
  string MainCurrency;
  decimal? ExpenseVariationPct; decimal? IncomeVariationPct;  // null si mes previo = 0
  List<TopCategoryItem> TopCategories;    // top 5 mes actual, moneda principal, con %
  int MovementCount;
  string PeriodLabel; // "octubre 2026" para el prompt
}
```
Reglas: todas las queries filtran `Account.UserId == userId`; montos `decimal`; mes = año/mes UTC actual (`DateTime.UtcNow`); top categorías con "Sin categoría" cuando `Category == null`; variación `null` si previo = 0 (igual que `ComparisonsResponse.Variation`).

## 4. System prompt (esqueleto, español)
```
Sos el asistente financiero de Finova. Respondé en español, breve y claro.
ÚNICA fuente de verdad (JSON snapshot calculado por el backend): <snapshot serializado con 2 decimales>.
REGLAS: (1) Usá SOLO cifras del snapshot, no inventes ni estimes. (2) Si el dato no está o HasData=false, decí "Aún no tenés datos registrados" o "No tengo ese dato". (3) Consejos ("cómo reduzco gastos") solo a partir de TopCategories reales. (4) Pregunta fuera de finanzas personales → derivala a finanzas o decí que no podés ayudar. (5) Nunca menciones otros usuarios ni datos ajenos.
Pregunta del usuario: <question>
```

## 5. Archivos a crear / modificar
**Backend (crear):**
- `backend/Finova.Application/DTOs/AssistantDtos.cs` — `ChatRequest { string Question }`, `ChatResponse { string Answer }`, `AssistantSnapshot` (+ reutilizar `CurrencyTotal`, `MonthTotalItem`, `TopCategoryItem` existentes).
- `backend/Finova.Application/Interfaces/IAssistantService.cs` — `Task<ChatResponse> GetAnswerAsync(Guid userId, string question)`.
- `backend/Finova.Infrastructure/Services/AssistantService.cs` — construcción snapshot (copiar/adaptar queries de `DashboardService.GetAsync` + `GetComparisonsAsync` limitadas a moneda principal) + llamada Ollama con `HttpClient` + `IOptions<OllamaOptions>`/lectura `IConfiguration["Ollama:BaseUrl/Model"]` + `try/catch (HttpRequestException, TaskCanceledException)` → lanzar `AssistantUnavailableException`.
- `backend/Finova.API/Controllers/AssistantController.cs` — `[Authorize] POST /api/assistant/chat`, `GetUserId()` igual que `DashboardController`; valida `question` no vacía (400); captura `AssistantUnavailableException` → 503 mensaje spec.
**Backend (modificar):**
- `backend/Finova.API/Program.cs` — registrar `IAssistantService`, `AddHttpClient<IAssistantService, AssistantService>(timeout 100s)`; NO cambiar CORS/auth existentes.
- `backend/Finova.API/appsettings.json` — agregar sección `"Ollama": { "BaseUrl": "http://localhost:11434", "Model": "llama3.1:8b" }` (sin secretos; valores locales por defecto).
**Frontend (crear):**
- `frontend/app/asistente/page.tsx` — `"use client"`; lista de mensajes en estado (`{role, text}[]`), input + botón Enviar (MUI, responsive `Container maxWidth="sm"`), `apiFetch("/api/assistant/chat", {method POST})`; si 401 → `router.push("/login")`; si 503 → burbuja de error "Asistente no disponible, intentá más tarde"; loading "Pensando…"; textos en español; enlace Volver.
**Frontend (modificar):**
- Navegación existente (home y/o dashboard): agregar botón/enlace "Asistente" hacia `/asistente`.
**Docs/entorno (no código):**
- `README` o nota en plan/tasks: prerrequisitos (ver §8).

## 6. DTOs de API
- Request: `POST /api/assistant/chat` body `{ "question": "¿cuánto gasté este mes?" }` (máx. p.ej. 500 caracteres → 400 si excede; Decisión pendiente: límite exacto).
- Response OK: `{ "answer": "..." }`. Error 503: `{ "message": "Asistente no disponible, intentá más tarde" }`.

## 7. Decisiones y alternativas descartadas
- **HttpClient directo vs SDK oficial de Ollama/NuGet:** se elige HttpClient directo (POST a `/api/chat` con `stream:false`, JSON manual). Motivo: cero dependencias nuevas, suficiente para respuesta completa, fácil timeout y manejo 503. SDK descartado (dependencia extra, sin beneficio en V1).
- **Respuesta completa vs streaming (SSE):** respuesta completa (`stream:false`). Motivo: UI simple tipo chat V1, evita complejidad SSE en Next.js/.NET. Streaming queda como mejora futura.
- **Snapshot multimilloneda completo vs moneda principal:** snapshot incluye saldos/totales en todas las monedas + detalle comparativo solo en moneda principal (la de mayor saldo). Motivo: prompt acotado, cubre las 5 preguntas sin ambigüedad; mezclar monedas en comparaciones confundiría al modelo. Detallar moneda en la respuesta.
- **Stateless (sin historial):** según spec fuera de alcance; cada request lleva solo system+pregunta actual. Historial visual solo en estado del front.
- **Modelo recomendado:** `llama3.1:8b` (liviano, buen español, corre en CPU/GPU modesta). Alternativa si poca RAM: `phi3:mini` o `mistral:7b`. Decisión pendiente: fijar el modelo exacto tras prueba local de calidad/latencia.

## 8. Prerrequisitos del entorno (el implementador debe documentarlos)
1. Instalar Ollama (https://ollama.com) y ejecutar `ollama pull llama3.1:8b` (o modelo que se fije) + `ollama serve` (puerto 11434). 2. Verificar `GET http://localhost:11434` responde. 3. `appsettings.Development.json`/user-secrets pueden sobreescribir `Ollama:BaseUrl` y `Ollama:Model` sin commitear secretos (no hay API key: Ollama local no requiere).

## 9. Seguridad y Constitución §7
- Snapshot construido SIEMPRE con `userId` del JWT; ninguna query sin filtro de usuario; el prompt nunca incluye datos de otros usuarios ni IDs ajenos.
- Cálculos en backend (`decimal`); el modelo solo redacta. Validar `question` no vacía/longitud máxima; no loguear snapshot completo en producción.
- 401 lo maneja el middleware JWT; el front redirige a login (patrón `dashboard/page.tsx`).

## 10. Casos de error
| Caso | Comportamiento |
|---|---|
| Sin token / token inválido | 401 (automático); front → /login |
| Pregunta vacía o > límite | 400 "Escribí una pregunta." |
| Ollama caído / timeout / modelo inexistente | 503 "Asistente no disponible, intentá más tarde" |
| Sin movimientos (HasData=false) | 200 con aviso "aún no hay datos", sin cifras inventadas |
| Pregunta no financiera | 200 derivando a finanzas / "no puedo ayudar con eso" |

## 11. Estrategia de pruebas
- **Las 5 preguntas contra datos reales (seed con 2–3 cuentas, ingresos/gastos en mes actual y anterior, ≥3 categorías):** (1) "¿cuánto gasté este mes?" = `MonthExpenses`; (2) "¿en qué gasté más?" = `TopCategories[0]`; (3) "¿cuánto tengo en total?" = `TotalBalances`; (4) "¿gasté más que el mes pasado?" = comparación + `ExpenseVariationPct`; (5) "¿cómo reduzco mis gastos?" menciona top categorías reales. Contrastar cifras con `/api/dashboard` e historial.
- **No-invención:** usuario nuevo sin movimientos → respuesta sin cifras; pregunta por dato ausente (p.ej. "¿cuánto gasté en viajes en 2020?") → "no tengo ese dato".
- **Aislamiento:** dos usuarios con datos distintos → respuestas no se cruzan.
- **Errores:** sin token → 401; Ollama detenido → 503 mensaje exacto; pregunta vacía → 400.
- **Validaciones:** `dotnet build` backend, `npm run build`/`tsc` front, responsive móvil (input visible, burbujas legibles), sin secretos en repo.

## 12. Trazabilidad con la spec
- 5 preguntas reales → §2/§11; cálculos en backend → §3; 401 → §9/§10; sin secretos fuera del repo → §8 (Ollama local, sin keys); español → §4/front; <15s + timeout claro → HttpClient 100s máx./mensaje 503 (§5/§10); sin movimientos no inventa → HasData (§2/§10); fuera de finanzas deriva → prompt regla 4; fuera de alcance (sin historial persistente/acciones/voz) respetado → §7 stateless.

## 13. Riesgos y decisiones pendientes
- **Decisión pendiente:** modelo exacto (`llama3.1:8b` propuesto) y límite de longitud de pregunta; timeout final (60 vs 100 vs 120s) según latencia medida (<15s requerido por spec; Ollama en CPU puede tardar — medir y ajustar).
- Riesgo: primera respuesta lenta por carga del modelo en memoria (warm-up con `ollama run` previo o request inicial lenta); documentar en pruebas.
- Riesgo: alucinaciones del modelo aun con prompt restrictivo — mitigar con temperatura baja (`options: { temperature: 0.2 }`) y validación manual de las 5 preguntas.
- Sin cambios de modelo de datos ni migraciones (no se persiste nada del chat).
