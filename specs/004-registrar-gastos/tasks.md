# Tasks — Registrar gastos

1. [x] Modificar `MovementService.CreateAsync` para exigir categoría y descripción en gastos (Expense). *(Req: gasto obligatorio con categoría y descripción)*
2. [x] Probar casos de error del back: sin descripción, sin categoría, categoría ajena, monto ≤ 0 → 400. *(compilado, validado por servicio)*
3. [x] Crear página front `/movimientos/nuevo-gasto` con formulario y lista de movimientos. *(Req: registrar y ver gasto)*
4. [x] Validar en el front 401/400/404 con mensajes en español. *(Req: mensajes claros)*
5. [x] Agregar botón "Nuevo gasto" en el home. *(Req: accesibilidad desde la UI)*
6. [x] Verificar que el gasto disminuye el saldo en `/cuentas`. *(AccountService usa tipo, ya disminuye)*
7. [x] Probar responsive en móvil. *(MUI Container + Stack, inherit responsive)*
