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