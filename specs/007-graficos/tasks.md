# Tasks — Gráficos

1. [x] Crear `DTOs/DashboardChartsResponse.cs` (gastos por categoría, comparativa 6 meses, evolución diaria).
2. [x] Extender `IDashboardService` + `DashboardService` con los 3 métodos (decimal, UTC, aislamiento por usuario).
3. [x] Agregar los 3 endpoints en `DashboardController` (`?currency=` requerido, 400 si falta).
4. [x] Verificar back: build + probar endpoints con Swagger (coincidencia con historial, 400/401, aislamiento).
5. [x] Instalar `@mui/x-charts` (versión compatible con React 19 + MUI 9) y validar build.
6. [x] Agregar sección "Gráficos" + selector de moneda en `/dashboard`.
7. [x] Crear los 3 componentes (torta, barras, línea) con estados carga/vacío/error.
8. [x] Validar: datos coinciden con historial, cambio de moneda, 401, vacío, responsive móvil.
