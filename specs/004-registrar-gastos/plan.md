# Plan — Registrar gastos

## Cambios en backend
- `Infrastructure/Services/MovementService.cs`: cuando `Type == Expense` exigir `CategoryId` y `Description` (400 si faltan); la validación de categoría del usuario y tipo Expense ya existía.
- `DTOs/CreateMovementRequest.cs`: `CategoryId` y `Description` siguen siendo opcionales a nivel modelo (la obligatoriedad la aplica el servicio según el tipo).
- No se crean nuevos endpoints: `POST /api/movements` ya acepta Expense.

## Cambios en frontend
- Nueva página `/movimientos/nuevo-gasto` (misma estructura que `/movimientos/nuevo` pero para gastos):
  - Cuenta, monto, fecha, categoría (obligatoria, `type=2` en `GET /api/categories`), descripción (obligatoria).
  - Errores 400/404/401 con mensaje en español.
  - Lista de movimientos mostrada tras registrar.
- Home: botón "Nuevo gasto" junto a "Nuevo ingreso".

## Flujo de datos
`POST /api/movements` con `Type=2` → `MovementService` valida → `Movement` (Expense) → `GET /api/accounts/{id}` muestra saldo disminuido.

## Estrategia de pruebas
- Crear gasto → 201, movimiento con tipo Gasto, saldo de cuenta disminuye.
- Gasto sin descripción → 400.
- Gasto sin categoría → 400.
- Gasto con categoría de otro tipo → 400.
- Cuenta ajena → 404.
- Sin token → 401.

## Relación con requisitos
- Expense obligatorio categoría+descripción: regla en MovementService.
- Saldo disminuye: cálculo derivado en AccountService (ya).
- Códigos de error: MovementsController (ya).

## Decisiones
- Reutilizar el endpoint `POST /api/movements` existente en vez de crear uno nuevo por tipo (consistencia con ingresos).
