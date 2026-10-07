# MEMORY.md — Finova

Memoria del proyecto entre sesiones.
Máximo ~50 líneas. Mantener solo información que siga siendo relevante.

## Estado actual

- Proyecto creado desde cero.
- AGENTS.md creado con las reglas y convenciones del proyecto.
- Arquitectura definida: Next.js + MUI + React Icons en frontend y .NET en backend.
- Backend creado con Clean Architecture: Finova.API, Finova.Application, Finova.Domain, Finova.Infrastructure.
- Entidades creadas: User, Account, Category, Movement (Ids Guid, MovementType único Income/Expense).
- FinovaDbContext configurado con Npgsql + migración inicial creada.
- Connection string de Neon ya pegada en appsettings.Development.json y migración inicial aplicada.
- Autenticación con JWT implementada: registro, login, generador de tokens y Swagger con Bearer.
- Paquetes: Swashbuckle, JwtBearer, ApplicationInsights, EFCore.Tools en API; Identity.Core (ex-Microsoft.AspNetCore.Identity) en Infrastructure.
- Todavía no hay endpoints de movimientos.
- Ítem 1 (registro/login) completo: pantallas front + back, cierre de sesión del lado cliente.
- Ítem 2 (cuentas): back (crear/listar con saldo calculado) + front (/cuentas) listo.
- Ítem 3 (ingresar dinero) implementado: MovementsController (POST/GET), MovementService, DTOs, CategoriesController, página /movimientos/nuevo.
- Ítem 4 (registrar gastos) implementado siguiendo SDD: spec/plan/tasks en specs/004-registrar-gastos, MovementService con obligatoriedad de categoría+descripción en Expense, página /movimientos/nuevo-gasto.
- Ítem 5 (historial) implementado y validado: GET /api/movements/history con filtros (fecha/cuenta/categoría/tipo) y saldo resultante por fila; página /movimientos.
- Categorías del sistema: 15 predefinidas (UserId null) vía migración SystemCategories; gasto con categoría opcional (descripción sigue obligatoria); validaciones aceptan sistema o propias; specs 004/009/010 y AGENTS.md actualizados.
- Ítem 6 (dashboard) implementado y verificado: GET /api/dashboard (saldos por moneda, ingresos/gastos del mes calendario UTC, últimos 5) + página /dashboard + botón en home; builds backend y frontend OK.
- Ítem V2.1 (gráficos, spec 007) implementado: 3 endpoints GET /api/dashboard/{expenses-by-category,income-vs-expenses,balance-evolution}?currency= (decimal, UTC, aislamiento por usuario, 400 sin moneda) + sección "Gráficos" en /dashboard con selector de moneda y 3 componentes MUI X Charts v9 (torta/barras si >8, barras 6 meses, línea saldo acumulado con arrastre); builds backend y frontend OK.
- Ítem V2.2 (comparaciones, spec 008) implementado: endpoint agregador GET /api/dashboard/comparisons?currency= (mes actual vs. anterior con variación % nullable, top 5 con %, evolución 6 meses con ceros; decimal, UTC, aislamiento por usuario) + sección "Comparaciones" en /dashboard (tarjetas mes vs. anterior, torta top 5, barras evolución) reutilizando selector de moneda; builds backend y frontend OK.
- Ítem V2.3 (presupuestos, spec 009) implementado y verificado: entidad Budget + migración AddBudgets (solo CREATE TABLE, aplicada en Neon) + BudgetService (spent del historial, umbrales 80/100, PUT solo monto) + BudgetsController + página /presupuestos; smoke test OK (duplicado→400, monto 0→400, Acercandose con 83.33%, ajeno→404, 401).
- Ítem V2.4 (metas, spec 010) implementado y verificado: entidad SavingGoal + GoalId nullable en Movement (SetNull) + migración AddSavingGoalsWithContributionLink (aplicada en Neon) + SavingGoalService (progreso derivado, En curso/Cumplida/Vencida, aporte = gasto "Aporte a meta {nombre}") + SavingGoalsController (api/savinggoals) + página /metas; smoke test OK (fecha pasada→400, aporte descuenta saldo y figura en historial, eliminar desvincula sin alterar saldos, 401/404).
- Ítem V3 (asistente IA, spec 011) implementado y verificado: AssistantService (snapshot por usuario con decimal/UTC/moneda principal + system prompt anti-invención + HttpClient a Ollama llama3.1:8b, temperature 0.2, timeout 100s) + POST /api/assistant/chat ([Authorize], 400 pregunta vacía/larga, 503 "Asistente no disponible, intentá más tarde") + página /asistente (stateless, sugerencias, 401→login) + enlace en home; smoke test OK (5 preguntas con cifras del dashboard, sin datos no inventa, dato ausente→"No tengo ese dato", 401, 503).
- Spec 013 (proveedor Gemini V3.1) implementada y validada E2E: `GeminiChatClient` (REST v1beta, gemini-3.8-flash, temperature 0.2; sin key/429/vacía → 503) + factory por `AiProvider` (Ollama default local, Gemini en prod); las 5 preguntas responden cifras reales vía Gemini; key solo en user-secrets/App Settings, nunca en repo.
- Refactor 012 (UseCases + Repositories + Tests) implementado y verificado: 8 interfaces repo en Application/Interfaces (IUser/IAccount/ICategory/IMovement/IBudget/ISavingGoalRepository + IPasswordHasher + IChatClient), 8 implementaciones en Infrastructure/Repositories (EF + PasswordHasherAdapter + OllamaChatClient), 24 use cases en Application/UseCases (validaciones solo acá), controllers finos con mismo mapeo HTTP, 7 Services viejos eliminados, proyecto Finova.Tests (xUnit+Moq+FluentAssertions, 70 tests en verde); build solución 0 errores; smoke test OK (register/login, dashboard, comparisons, categories, cuenta+movimiento con saldo 5000).
- Repo publicado en GitHub: https://github.com/GastonEE01/Finova (privado, branch master).
- Pulidos review 012 aplicados: eliminado UnitTest1.cs placeholder, SumAsync con await en MovementRepository, GetDashboardUseCase con una sola carga de movimientos, JwtTokenGenerator movido a Infrastructure/Security; build 0 errores, 69 tests en verde.
- Diseño "Bosque y moneda" aplicado a gráficos: paleta propia por modo vía useChartPalette (esmeralda/rojo/oro + rampa categórica), tortas como dona con esquinas redondeadas, barras redondeadas, línea con área suavizada, tooltips en formato es-AR.
- Landing pública /bienvenida (hero Ink + maqueta del panel en código, funciones, 3 pasos, FAQ, CTA final; layout con metadata SEO); sin imágenes IA por balance $0 en belt.
- AppShell: AppBar (Finova, nav desktop, +Nuevo, toggle tema, avatar+email, salir) + bottom nav móvil (Panel, Movimientos, [+] chooser, Cuentas, Ayuda); páginas protegidas en grupo (app); / redirige a /dashboard (Panel con saludo).
- Cuentas: combo de 29 monedas (CURRENCIES); historial: filtro de categoría con sufijo (ingreso/gasto) para distinguir "Otros" duplicado.
- Limpieza de datos: 11 categorías de usuario duplicadas eliminadas en Neon (movimientos reasignados a las del sistema, sin pérdida).
- `gh` CLI instalado; auth vía navegador. Secretos Neon/JWT en `dotnet user-secrets`, no commiteados.

## Decisiones

- La aplicación tendrá interfaz en español.
- La aplicación será responsive y tendrá como prioridad la experiencia móvil.
- Las finanzas pertenecen a cada usuario y deben mantenerse aisladas.
- La moneda se define a nivel de cuenta.
- La IA analizará datos reales obtenidos desde el backend y no podrá inventar información financiera.

## Aprendizajes y errores a evitar

- (vacío por ahora)

## Próximos pasos

- Diseñar las entidades principales del dominio. ✅
- Configurar la base de datos (Npgsql + DbContext + migración). ✅ Aplicada en Neon.
- Implementar autenticación. ✅ (JWT: register/login, Swagger Bearer)
- Implementar CRUD de cuentas y movimientos.