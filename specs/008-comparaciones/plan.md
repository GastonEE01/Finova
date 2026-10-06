# Plan técnico — 008 Comparaciones (roadmap V2 tarea 2)

## 1. Objetivo y alcance
Agregar a `/dashboard` una sección "Comparaciones" que muestre, para una moneda por consulta (`?currency=`, sin conversión):
1. Mes actual vs. mes anterior (ingresos y gastos, con variación %).
2. Top 5 categorías por gasto del mes actual (monto + % del total).
3. Evolución de gastos mensual de los últimos 6 meses.

Sin conversión de moneda, sin comparación por cuenta individual, sin períodos personalizados, sin presupuestos/metas (V2.3/2.4). Spec aprobada `spec.md` NO se modifica.

## 2. Arquitectura y reutilización
- Patrón 007-gráficos: `DashboardController ([Authorize]) → IDashboardService → DashboardService (EF Core, filtro `m.Account.UserId == userId` + `m.Account.Currency == currency`) → DTOs en `Finova.Application/DTOs` → frontend `frontend/app/dashboard/page.tsx` + componentes MUI X Charts v9 con selector de moneda existente.
- Ventanas temporales en UTC igual que 007: mes calendario UTC (día 1 00:00 UTC → `DateTime.UtcNow`); 6 meses = desde el día 1 del mes actual menos 5 meses. Comparación mensual usa `m.Date.Year/Month` en UTC, igual que `DashboardService.GetExpensesByCategoryAsync` / `GetIncomeVsExpensesAsync`.
- Reutilizar: normalización de moneda (`currency.Trim().ToUpper()`), `400 "Debe indicar la moneda."` sin moneda, `401` por `[Authorize]` + redirect a `/login` en frontend, mensajes de vacío/error en español, layout responsive (`Container maxWidth="sm"`, columna con `gap`).

## 3. Decisión principal: un endpoint nuevo agregador
- **Decisión: crear UN endpoint nuevo `GET /api/dashboard/comparisons?currency={MONEDA}`** que devuelva las 3 piezas en una sola respuesta.
- **Alternativa descartada A (reutilizar solo endpoints 007):** componer `expenses-by-category` + `income-vs-expenses` en frontend y calcular variación % y top-5/% en el cliente. Descartada porque viola constitución §2 y §4 (cálculos financieros en backend; decisiones de negocio en backend) y duplica lógica de ventanas UTC en dos capas.
- **Alternativa descartada B (3 endpoints nuevos separados):** un endpoint por pieza. Descartada por triple roundtrip y triple filtrado para datos que salen de las mismas 2 consultas base; el agregador hace 2 queries y deriva todo en memoria.
- El endpoint nuevo reutiliza internamente la misma lógica de agregación de 007 (no duplica ventanas ni filtros).

## 4. Backend

### 4.1 Archivos
| Archivo | Acción |
|---|---|
| `backend/Finova.Application/DTOs/DashboardComparisonsResponse.cs` | **CREAR** (nuevo; requiere aprobación Coordinator por AGENTS.md "preguntar antes de crear archivos") |
| `backend/Finova.Application/Interfaces/IDashboardService.cs` | MODIFICAR: agregar `Task<ComparisonsResponse> GetComparisonsAsync(Guid userId, string currency)` |
| `backend/Finova.Infrastructure/Services/DashboardService.cs` | MODIFICAR: implementar `GetComparisonsAsync` |
| `backend/Finova.API/Controllers/DashboardController.cs` | MODIFICAR: agregar `GET comparisons` con validación `400` |

### 4.2 Contrato propuesto
```
GET /api/dashboard/comparisons?currency=ARS → 200
{
  "currency": "ARS",
  "currentMonth":  { "year": 2026, "month": 10, "income": 150000.00, "expense": 90000.00 },
  "previousMonth": { "year": 2026, "month": 9,  "income": 120000.00, "expense": 0 },
  "incomeVariationPct": 25.00,      // decimal? null si mes anterior en 0
  "expenseVariationPct": null,      // → frontend muestra "—"
  "topCategories": [
    { "categoryId": "guid|null", "categoryName": "Supermercado", "total": 30000.00, "percent": 33.33 }
  ],                               // máx 5, orden desc; vacío [] si sin gastos
  "expenseEvolution": [
    { "year": 2026, "month": 5, "expense": 0 }, ... 6 items
  ]
}
```
Errores: `400` si `currency` vacía (`"Debe indicar la moneda."`); `401` sin autenticación (automático por `[Authorize]`).

### 4.3 DTOs (nombres en inglés, `decimal` para dinero, `decimal?` para variación)
- `MonthTotalItem { int Year; int Month; decimal Income; decimal Expense; }` (o reutilizar estructura similar a `MonthlyTotalItem` si el implementador prefiere; no agregar `Income` a la evolución que solo lleva gasto).
- `TopCategoryItem { Guid? CategoryId; string CategoryName; decimal Total; decimal Percent; }` — `CategoryName ?? "Sin categoría"`.
- `ExpenseEvolutionItem { int Year; int Month; decimal Expense; }`.
- `ComparisonsResponse { string Currency; MonthTotalItem CurrentMonth; MonthTotalItem PreviousMonth; decimal? IncomeVariationPct; decimal? ExpenseVariationPct; List<TopCategoryItem> TopCategories; List<ExpenseEvolutionItem> ExpenseEvolution; }`.

### 4.4 Cálculos (todo en backend, `decimal`, sin `float`/`double`)
1. Normalizar `currency = currency.Trim().ToUpper()`.
2. `now = DateTime.UtcNow`; `curStart = 1° del mes actual UTC`; `prevStart = curStart.AddMonths(-1)`; `evoStart = curStart.AddMonths(-5)`.
3. Una query: movimientos del usuario + moneda con `m.Date >= evoStart` (proyectar `{ Type, Amount, Date, CategoryId, CategoryName }`). Todo filtrado por `m.Account.UserId == userId`.
4. Particionar en memoria: mes actual (`Year/Month == now`), mes anterior (`== prevStart`), cada uno de los 6 meses para evolución (faltantes → `0`, no omitir el punto).
5. `Income/Expense` = `Sum` por `MovementType` (colección vacía → `0`, nunca null).
6. Variación % por tipo: `previous == 0 ? (decimal?)null : Math.Round((current - previous) / previous * 100, 2)`. **Nunca dividir por cero.** Cero actual vs. anterior > 0 → `-100%` (válido). Redondeo explícito a 2 decimales.
7. Top 5: agrupar gastos del mes actual por `{ CategoryId, CategoryName ?? "Sin categoría" }`, `OrderByDescending(Total).Take(5)`; `monthTotal = suma`; `Percent = monthTotal == 0 ? 0 : Math.Round(item / monthTotal * 100, 2)`.
8. Sin conversión: solo `m.Account.Currency == currency` (comparación exacta como en 007; moneda es la de la cuenta, y los movimientos usan la de su cuenta por regla de dominio).

### 4.5 Seguridad / validaciones / errores
- Aislamiento: todos los accesos filtran por `userId` del claim (`GetUserId()` existente); categorías/movimientos ajenos quedan excluidos por el join a cuentas propias.
- `400` moneda ausente; `401` sin token; moneda inexistente para el usuario → respuesta `200` con totales en `0` y listas vacías (mismo comportamiento que 007), no `404`.
- Validar que `Percent`/`VariationPct` serialicen como número o `null` (no `NaN`/`Infinity`).

## 5. Frontend

### 5.1 Archivos
| Archivo | Acción |
|---|---|
| `frontend/app/dashboard/comparisons-section.tsx` | **CREAR** (orquestador; requiere aprobación por AGENTS.md) |
| `frontend/app/dashboard/month-comparison-cards.tsx` (o dentro del anterior) | CREAR opcional — fusionable en `comparisons-section.tsx` si se quiere un solo archivo nuevo |
| `frontend/app/dashboard/top-categories-chart.tsx` | CREAR (top 5) |
| `frontend/app/dashboard/expense-evolution-chart.tsx` | CREAR (evolución 6 meses) |
| `frontend/app/dashboard/page.tsx` | MODIFICAR: agregar sección `<ComparisonsSection currency={selectedCurrency} />` debajo de "Gráficos", reutilizando `selectedCurrency` y sin nuevo selector |

Nota: si el Coordinator prefiere minimizar archivos nuevos, los 3 componentes pueden vivir en un único `comparisons-section.tsx`. Nombres de variables/textos en español para el usuario; código en inglés.

### 5.2 Responsabilidad por componente
- `page.tsx`: solo monta `<ComparisonsSection currency={selectedCurrency} />` cuando hay `selectedCurrency` (igual que los charts 007). Sin fetch nuevo aquí.
- `comparisons-section.tsx`: único fetch a `/api/dashboard/comparisons?currency=...` (patrón `apiFetch` + `getToken`, `401 → /login`, `setData(null)/setError("")` al cambiar moneda); renderiza 3 bloques con estados `Cargando… / error / vacío`.
- Tarjetas mes vs. anterior: 2 tarjetas (Ingresos / Gastos) con `actual`, `anterior` y variación: `null → "—"` (gris, "sin datos previos"), `> 0` verde con `+`, `< 0` rojo, `0` neutro "sin cambios". Formato `toFixed(2)` + moneda.
- Top 5: si `[]` → `"Sin gastos este mes."`; si no, `PieChart` (≤5 siempre torta; el umbral >8 de 007 no aplica) o lista con barra; cada item `nombre · monto · %`.
- Evolución: `BarChart` solo-gastos 6 meses con `MONTH_LABELS` existente; todos en 0 → `"Sin movimientos en el período."` (mismo texto que 007).

### 5.3 Textos (español) y responsive
- Título sección: `"Comparaciones"`; subtítulos: `"Este mes vs. mes anterior"`, `"Dónde gastás más (top 5)"`, `"Evolución de gastos (últimos 6 meses)"`.
- Reutilizar `Box` con `flexDirection: column, gap`; charts con `height={280}`; verificar móvil 360px sin scroll horizontal.

## 6. Flujo de datos (resumen)
```
ComparisonsSection (currency) → GET /api/dashboard/comparisons?currency=
→ [Authorize] → DashboardController → DashboardService.GetComparisonsAsync(userId, currency)
→ 1 query EF (usuario+moneda, desde hace 5 meses) → agregación decimal en memoria
→ ComparisonsResponse → frontend renderiza tarjetas + top5 + evolución
```

## 7. Estrategia de pruebas (sin datos de prueba, con datos reales existentes)
1. `dotnet build` backend + `npm run build` (o `next lint`/`tsc`) frontend OK.
2. Manual con usuario real: `GET comparisons?currency=` con moneda con datos → cruzar `currentMonth`/`previousMonth` contra `/movimientos` filtrado por mes; spot-check de `%` con calculadora.
3. Casos límite: mes anterior en 0 → `"—"` (sin error 500); mes actual sin gastos → top vacío con mensaje; usuario con < 6 meses → meses faltantes en 0; moneda sin datos → todo en 0/vacío; sin `currency` → `400`; sin token → `401` + redirect; segundo usuario no ve datos del primero.
4. Responsive: 360px y desktop; textos en español; sin secretos en commits.

## 8. Trazabilidad con la spec
| Spec § | Plan |
|---|---|
| Mes actual vs. anterior con variación % por moneda | `currentMonth/previousMonth + *VariationPct`, tarjetas, `null → "—"` |
| Top 5 con monto + % | `topCategories` (Take 5, `Percent`), torta/lista |
| Evolución 6 meses | `expenseEvolution` (6 items, faltantes 0), `BarChart` |
| `401` / solo datos propios | `[Authorize]`, filtro `UserId`, test multi-usuario |
| `decimal`, división por cero controlada | §4.4 (pasos 5–7) |
| UI español, responsive, MUI X Charts, patrón dashboard | §5 |
| Sin seed, verificar con datos reales | §7 |
| Fuera de alcance (presupuestos, por cuenta, períodos custom) | No se incluye |

## 9. Riesgos y decisiones pendientes (requieren aprobación del Coordinator)
1. **Aprobación para crear archivos nuevos** (AGENTS.md): `DashboardComparisonsResponse.cs`, `comparisons-section.tsx`, `top-categories-chart.tsx`, `expense-evolution-chart.tsx` (o versión fusionada en 1 archivo frontend). Sin esto no se implementa.
2. **Decisión pendiente — granularidad del `Percent` del top:** redondeo a 2 decimales puede sumar 99.99/100.01; se propone aceptarlo (spot-check manual), sin ajuste de residuo.
3. **Decisión pendiente — reutilización de `MonthlyTotalItem` vs. DTOs nuevos:** se propone DTOs nuevos dedicados para no acoplar evolución/top a la forma de 007; confirmar.
4. Riesgo bajo: fechas futuras dentro del mes actual se incluyen (heredado de 007); documentado, no se cambia en esta tarea.
