# Plan — Refactor a UseCases + Repositories + Tests (012)

## Objetivo
Separar la lógica de negocio en `Application/UseCases`, abstraer persistencia en `Application/Interfaces` + `Infrastructure/Repositories`, dejar controllers finos y cubrir con tests xUnit (3 por use case: feliz, validación, aislamiento).

## Orden
1. `Application/Interfaces/`: `IUserRepository`, `IAccountRepository`, `ICategoryRepository`, `IMovementRepository`, `IBudgetRepository`, `ISavingGoalRepository` — métodos con intención (ej. `GetByUserAsync`, `ExistsByEmailAsync`), no CRUD genérico.
2. `Infrastructure/Repositories/`: implementaciones con EF sobre `FinovaDbContext`.
3. `Application/UseCases/`: un caso de uso por operación (Auth: Register/Login; Accounts: Create/List/Get; Movements: Create/List/History; Dashboard: Get/Charts/Comparisons; Budgets CRUD; Goals CRUD + Contribute; Assistant: Ask). Mueven la lógica desde los Services; validaciones solo acá.
4. Controllers finos: llaman al use case y mapean excepciones a HTTP (patrón existente).
5. `Infrastructure/Services/` viejos: se eliminan cuando su lógica migre (verificar que nada los referencie).
6. `backend/Finova.Tests` (xUnit + Moq + FluentAssertions + Test.Sdk + runner): 3 tests por use case con repos mockeados.
7. DI en `Program.cs` (repos + use cases); `dotnet build`; `dotnet test`; smoke test API.

## Reglas
- Sin cambios de entidades, migraciones ni contratos de API (mismos endpoints/DTOs/errores).
- Sin secretos nuevos. Comportamiento observable idéntico (verificar con smoke test).
