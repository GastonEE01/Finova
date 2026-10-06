# Plan técnico — 010 Metas de ahorro (V2.4, rev. 2)

Spec: `specs/010-metas-ahorro/spec.md` (ACTUALIZADA, manda). Este plan reemplaza al anterior.
Cambios incorporados: fecha objetivo obligatoria y futura; aportes vinculados a cuenta como gastos;
`GoalId` nullable en Movement (SetNull); progreso derivado; sin bloqueo por saldo insuficiente;
ruta `api/savinggoals`; PUT valida fecha igual que POST; estado "Vencida".

## 1. Resumen
CRUD de metas de ahorro + aportes que crean un `Movement` de tipo gasto vinculado a la meta.
Sin `CurrentAmount` persistido: el progreso se deriva en backend como suma de aportes vinculados.
Nueva entidad `SavingGoal` + migración que (a) crea tabla `SavingGoals` y (b) agrega columna
nullable `GoalId` a `Movements` con FK SetNull. Cálculos en `decimal`, fechas UTC.
Página `/metas` con MUI, español, responsive, barras de progreso y estados En curso / Cumplida / Vencida.

## 2. Entidades y migración
### 2.1 Nueva entidad `SavingGoal` (Domain, nombres en inglés)
- `backend/Finova.Domain/Entities/SavingGoal.cs` (NUEVO):
  - `Guid Id`, `Guid UserId`, `string Name` (requerido, max 100, trim),
    `decimal TargetAmount` (> 0), `DateTime TargetDate` (REQUERIDA, UTC, futura al crear/editar),
    `DateTime CreatedAt` (UTC).
  - Navegaciones: `User User`, `List<Movement> Contributions` (aportes vinculados).
  - SIN `CurrentAmount`: el progreso es derivado. Derivados calculados en servicio/DTO:
    `Progress = SUM(movements WHERE GoalId = meta)`, `Remaining = Target - Progress`,
    `Percent = Progress / Target * 100`, `Status`: `Cumplida` si `Progress >= Target`;
    si no, `Vencida` si `hoy UTC > TargetDate (fecha)`; si no, `En curso`.
    Prioridad: Cumplida prevalece sobre Vencida (meta alcanzada aunque haya pasado la fecha).

### 2.2 Cambio en `Movement` (Domain)
- `backend/Finova.Domain/Entities/Movement.cs` (MODIFICAR): agregar
  `Guid? GoalId` (nullable) + navegación `SavingGoal? SavingGoal`.
  Nullable porque la mayoría de los movimientos no son aportes, y al eliminar la meta
  los aportes se conservan como gastos normales desvinculados (SetNull).

### 2.3 Fluent API en `FinovaDbContext`
- `DbSet<SavingGoal> SavingGoals`.
- `SavingGoal`: `Name IsRequired MaxLength(100)`; `TargetAmount HasPrecision(18,2)`;
  `TargetDate IsRequired`; índice en `UserId`; FK `SavingGoal → User` Cascade
  (consistente con Accounts/Categories); `User` suma colección `List<SavingGoal>`.
- `Movement`: FK `Movement → SavingGoal` con `HasForeignKey(m => m.GoalId)`
  `.OnDelete(DeleteBehavior.SetNull)` + índice en `GoalId`.
  No tocar la configuración existente de Account/Category.

### 2.4 Migración EF Core (toca Movements + tabla nueva)
- `dotnet ef migrations add AddSavingGoalsWithContributionLink --project Finova.Infrastructure --startup-project Finova.API`.
  Contenido esperado: `CREATE TABLE SavingGoals` + índice `UserId`;
  `ADD COLUMN GoalId uuid NULL` en `Movements` + índice + `FK Movements.GoalId → SavingGoals.Id SetNull`.
- Verificar el diff: no debe alterar ni perder columnas/datos existentes de Movements.
  Aplicar en Neon (InitialCreate ya aplicada). Rollback = remover migración si aún no aplicada;
  si ya aplicada, nueva migración que revierta (no editar migraciones aplicadas).

## 3. Archivos a crear / modificar
**Crear:**
- `backend/Finova.Domain/Entities/SavingGoal.cs`
- `backend/Finova.Application/DTOs/SavingGoalDtos.cs`
  (`CreateSavingGoalRequest { name, targetAmount, targetDate }`,
  `UpdateSavingGoalRequest { name, targetAmount, targetDate }`,
  `SavingGoalResponse { id, name, targetAmount, progress, remaining, percent, status, targetDate, ... }`,
  `AddContributionRequest { accountId, categoryId, amount }`)
- `backend/Finova.Application/Interfaces/ISavingGoalService.cs`
- `backend/Finova.Infrastructure/Services/SavingGoalService.cs`
- `backend/Finova.API/Controllers/SavingGoalsController.cs`
- `frontend/app/metas/page.tsx`
**Modificar:**
- `backend/Finova.Domain/Entities/Movement.cs` (+ `GoalId` + navegación)
- `backend/Finova.Domain/Entities/User.cs` (+ colección de metas)
- `backend/Finova.Infrastructure/Persistence/FinovaDbContext.cs` (+ DbSet y Fluent API §2.3)
- `backend/Finova.Infrastructure/Migrations/` (nueva migración §2.4)
- `backend/Finova.API/Program.cs` (registro DI del nuevo servicio, mismo patrón existente)
- Opcional: link en home/dashboard hacia `/metas`. Reutilizar `frontend/app/lib/auth.ts` sin cambios.
- Posible extensión menor: `CreateMovementRequest` con `GoalId` opcional interno
  (solo si se decide reutilizar `MovementService.CreateAsync` para el aporte; ver §5).

## 4. Endpoints y DTOs
Base `api/savinggoals`, `[Authorize] + GetUserId()` (patrón MovementsController).
Nombres de ruta en inglés; mensajes de error `{ mensaje }` en español; 401 sin autenticación.
- `GET /api/savinggoals` — lista propia con
  `id, name, targetAmount, progress, remaining, percent, status ("En curso"/"Cumplida"/"Vencida"), targetDate`.
- `POST /api/savinggoals` — body `{ name, targetAmount, targetDate }` (fecha obligatoria).
  400 si nombre vacío, objetivo ≤ 0, fecha ausente/inválida o fecha pasada (comparar por fecha UTC:
  `targetDate.Date <= hoy UTC → 400`). `createdAt = UtcNow`. Progreso inicial 0.
- `PUT /api/savinggoals/{id}` — edita `{ name, targetAmount, targetDate }`.
  Valida fecha IGUAL que POST (ausente/pasada → 400); objetivo > 0.
  Si el nuevo objetivo queda por debajo del progreso actual, se acepta (queda Cumplida con excedente).
  Ajeno/inexistente → 404.
- `DELETE /api/savinggoals/{id}` — propio. Gracias al FK SetNull, los movimientos de aporte
  se conservan como gastos normales desvinculados. Ajeno/inexistente → 404.
- `POST /api/savinggoals/{id}/contributions` — body `{ accountId, categoryId, amount }`.
  Crea un gasto vinculado (ver §5). 400 si monto ≤ 0, falta cuenta/categoría;
  404 si meta/cuenta inexistente o ajena; 400 si categoría inexistente/ajena o no es de tipo gasto.
  Respuesta: meta actualizada con cálculos + (recomendado) el `movementId` creado.
- Aislamiento: todos los accesos filtran por `UserId` del token
  (meta propia, cuenta propia, categoría propia). Cuenta o categoría ajena → 404/400 según
  el patrón vigente de MovementService (cuenta ajena = 404 `KeyNotFoundException`;
  categoría ajena o de tipo distinto = 400 `ArgumentException`).

## 5. Flujo del aporte (transacción única)
1. `SavingGoalService.AddContributionAsync(userId, goalId, { accountId, categoryId, amount })`:
   carga meta propia (si no → 404); valida `amount > 0` (si no → 400).
2. Valida cuenta propia (`Accounts WHERE Id + UserId`, si no → 404) y categoría propia
   de tipo gasto (`Categories WHERE Id + UserId + Type == Expense`, si no → 400).
   Sin validación de moneda (los movimientos usan la moneda de la cuenta) y
   SIN bloqueo por saldo insuficiente: la cuenta puede quedar negativa (spec).
3. Crea el `Movement`: `Type = Expense`, `AccountId`, `CategoryId`, `Amount`,
   `Date = UtcNow`, `Description = "Aporte a meta {nombre}"` (nombre actual de la meta),
   `GoalId = goalId`. Cumple las reglas de gasto de `MovementService`
   (categoría + descripción obligatorias).
4. Todo en UNA transacción EF (`SaveChanges` único / `ExecuteInTransaction`):
   insert del movimiento y recálculo del progreso por suma; si algo falla, rollback completo.
5. Reutilización: preferible reutilizar `MovementService.CreateAsync` si se extiende con
   `GoalId` opcional interno (evita duplicar validaciones cuenta/categoría/gasto);
   si no se extiende, `SavingGoalService` replica exactamente esas validaciones
   (documentar la duplicación como deuda menor). El aporte aparece automáticamente
   en historial, saldos y gráficos como gasto (descuenta saldo).
6. Progreso/estado (`GET`, `POST`, `PUT`, aporte): `progress = SUM(Movements WHERE GoalId)`,
   `remaining = Target - Progress`, `percent` redondeado a 2 decimales,
   `status` según §2.1. Aporte que supera el objetivo se acepta (Cumplida con excedente);
   también se acepta aportar a meta vencida o ya cumplida (la spec no lo prohíbe;
   el estado se recalcula).

## 6. Frontend `/metas`
- Patrón `cuentas/page.tsx`: `apiFetch`, redirect `/login` en 401, `Dialog` crear/editar
  (nombre, objetivo, fecha — input `date` requerido, validación cliente de fecha futura
  + manejo del 400 del backend), confirmación eliminar, `Dialog` de aporte
  (selector de cuenta propia, selector de categoría de gasto propia, monto > 0).
- Tarjeta/fila por meta: nombre, `progreso / objetivo`, `restante`, `LinearProgress`
  (value min(percent,100), color success si cumplida, error/warning si vencida),
  chip "En curso" / "Cumplida" / "Vencida", fecha objetivo (siempre presente).
  Excedente: "¡Meta cumplida! Excedente: X" cuando `progress > target`.
- Aclarar en la UI que el aporte descuenta de la cuenta elegida.
- Textos en español, `Container maxWidth="sm"`, responsive móvil.

## 7. Decisiones y alternativas
- `GoalId` nullable en Movement (ELEGIDO, exigido por spec actualizada) vs. entidad
  separada `Contribution` (DESCARTADO): `Contribution` daría historial aislado pero
  duplicaría el modelo de dinero y rompería el requisito de que el aporte descuente
  saldo y figure en historial/gráficos; `GoalId` reutiliza toda la maquinaria de
  movimientos (saldos, historial, gráficos, categorías) con un solo FK aditivo.
- Progreso derivado (ELEGIDO) vs. `CurrentAmount` manual (DESCARTADO, plan anterior):
  el campo manual se desincronizaría del gasto real; la suma vinculada es la única
  fuente de verdad coherente con "el aporte descuenta la cuenta".
- FK SetNull al eliminar (ELEGIDO) vs. Cascade (DESCARTADO): la spec exige conservar
  los movimientos como gastos normales; Cascade los borraría y alteraría saldos/historial.
- Cumplida prevalece sobre Vencida: evita que una meta alcanzada tarde se muestre como
  fracasada; coherente con "cumplida cuando progreso ≥ objetivo".
- Aportes a meta vencida/cumplida se aceptan: la spec solo pide aceptar el excedente
  y no define bloqueo; bloquear sería inventar un requisito.

## 8. Seguridad y validaciones
- `[Authorize]`; aislamiento por `UserId` en metas, cuentas y categorías; 404 ante ajeno.
- `decimal` (18,2) en montos; fechas UTC; comparación de fecha objetivo por fecha (no por instante).
- Nombre saneado (trim, max 100); descripción del aporte generada por backend
  (no aceptarla del cliente para garantizar el formato "Aporte a meta {nombre}").
- Sin cambios en el esquema de auth; no exponer datos de otros usuarios.

## 9. Estrategia de pruebas
- Backend build + Swagger/EF:
  crear (válida; objetivo 0 → 400; sin fecha → 400; fecha pasada → 400; fecha futura → 201);
  editar (fecha pasada → 400; objetivo por debajo del progreso → 200 y Cumplida);
  aporte (válido descuenta saldo y aparece en historial/gráficos como gasto con la
  descripción exacta; monto ≤ 0 → 400; cuenta ajena → 404; categoría ajena o de ingreso → 400);
  excedente → Cumplida con excedente; meta vencida (fecha pasada sin cumplirse) muestra Vencida;
  eliminar meta conserva movimientos desvinculados (`GoalId` NULL) y saldos intactos;
  aislamiento entre usuarios; 401 sin token.
- Migración: inspeccionar script (solo CREATE SavingGoals + ADD COLUMN GoalId + FK/índices);
  aplicar en Neon; verificar historial/saldos existentes intactos.
- Frontend build; responsive móvil; progress bar, chips de los 3 estados y excedente correctos; español.

## 10. Trazabilidad con la spec
- Crear (nombre, objetivo > 0, fecha futura obligatoria) → POST §4.
- Aportes (cuenta propia, monto > 0, categoría de gasto propia → gasto vinculado,
  descuenta cuenta) → `contributions` §4/5.
- Progreso/restante/%/estado (En curso/Cumplida/Vencida) → cálculo backend + UI §5/6.
- Editar (nombre, objetivo, fecha futura) y eliminar propias (desvincula movimientos) → PUT/DELETE §4.
- 401/404 y solo datos propios → §4/8.
- Casos límite (≤ 0 → 400, fecha pasada → 400 en crear y editar, excedente aceptado,
  cuenta/categoría ajena → 404/400) → §4/5/9.
- Fuera de alcance (retiros, recordatorios) → no se implementa.

## 11. Riesgos / decisiones pendientes
- **Resueltas por el usuario:** ruta `api/savinggoals`; PUT valida fecha igual que POST;
  fecha obligatoria; `GoalId` + SetNull; progreso derivado; sin bloqueo por saldo negativo.
- Riesgo bajo-medio: la migración toca la tabla `Movements` (con datos); exigir backup/
  verificación del diff antes de aplicar en Neon.
- Concurrencia en aportes simultáneos (lectura de suma + inserto): aceptable en V2.4;
  el progreso siempre se deriva por suma, por lo que no hay contador que corromperse;
  si se requiere estrictez, usar transacción con nivel serializable (ya previsto en §5).
- Duda menor a confirmar en implementación: ¿se permite aportar a meta vencida/cumplida?
  El plan asume SÍ (spec no lo prohíbe); si el Coordinator decide bloquearlo, actualizar
  spec §casos límite + este plan §5 antes de implementar.
