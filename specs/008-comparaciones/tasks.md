# Tasks — Comparaciones

1. [x] Crear `DTOs/DashboardComparisonsResponse.cs` (mes vs. anterior con variación %, top 5 con %, evolución 6 meses).
2. [x] Extender `IDashboardService` + `DashboardService` con `GetComparisonsAsync` (decimal, UTC, variación nula si mes anterior en 0).
3. [x] Agregar `GET /api/dashboard/comparisons?currency=` en `DashboardController` ([Authorize], 400 si falta moneda).
4. [x] Verificar back: build + probar con Swagger contra datos existentes (coincidencia con historial, 400/401, aislamiento).
5. [x] Crear sección `comparisons-section.tsx` + componentes `top-categories-chart.tsx` y `expense-evolution-chart.tsx` en `/dashboard`.
6. [x] Validar front: datos coinciden con historial, cambio de moneda, vacíos, 401, responsive móvil.
