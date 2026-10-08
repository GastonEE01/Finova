# Finova
Aplicación fullstack de gestión de finanzas personales donde se pueden registrar ingresos y gastos, administrar cuentas en distintas monedas, ver dashboard con gráficos y comparaciones, definir presupuestos y metas de ahorro, y consultar un asistente financiero con IA que responde con datos reales.

## 🔗 Enlaces del Proyecto
DEMO Frontend (Vercel): https://finova-coral.vercel.app/

API Backend (Azure): https://finovaapi-e2gse6bfhce5akep.brazilsouth-01.azurewebsites.net/api/dashboard

## Funcionalidades:
* Registro e inicio de sesión con JWT
* Cuentas con moneda (29 monedas) y saldo calculado
* Registro de ingresos y gastos con categoría opcional
* Historial de movimientos con filtros y saldo resultante
* Dashboard con saldos, totales del mes y últimos movimientos
* Gráficos por categoría, comparativa 6 meses y evolución del saldo
* Comparaciones mes actual vs. anterior y top de categorías
* Presupuestos por categoría con avisos de límite
* Metas de ahorro con aportes vinculados a cuentas
* Asistente financiero con IA (Ollama local / Gemini en producción)
* Modo oscuro / claro (global)
* Diseño responsive mobile-first

## Tecnologías Utilizadas

### Frontend:
* Next.js
* React
* MUI (Material UI)
* MUI X Charts
* React Icons

### Backend:
* .NET (ASP.NET Core Web API)
* Entity Framework Core
* Clean Architecture + UseCases + Repositories
* xUnit + Moq + FluentAssertions (69 tests)

### Base de Datos
* PostgreSQL
* Neon

### Deploy
* Vercel (Frontend)
* Azure App Service (Backend)

### Estado del proyecto
* Proyecto personal desarrollado con fines de práctica y aprendizaje Fullstack utilizando Next.js + .NET + Azure.

### Mejoras futuras:
* CRUD de categorías personalizadas
* Edición y eliminación de movimientos
* Filtros de período personalizados en gráficos
* Notificaciones de presupuestos
* Paginación del historial
