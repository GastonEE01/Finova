# Spec — Metas de ahorro (roadmap V2 tarea 4)

## Contexto y objetivo
Permitir al usuario crear metas de ahorro con un monto objetivo, registrar progreso y ver cuánto falta.

## Usuarios
- Usuario autenticado de Finova.

## Historias de usuario
- Como usuario, quiero crear una meta con nombre y monto objetivo.
- Como usuario, quiero registrar progreso (aportes) en mi meta.
- Como usuario, quiero ver cuánto me falta y mi % de avance.

## Definiciones
- Meta: nombre, monto objetivo (> 0), fecha objetivo OBLIGATORIA (futura al crear), cuenta de origen para los aportes.
- Aporte: movimiento de gasto en la cuenta elegida, con descripción "Aporte a meta {nombre}" y categoría de gasto a elección del usuario; descuenta el saldo de la cuenta y aparece en historial/gráficos como gasto.
- Progreso = suma de aportes vinculados (derivado, no editable). Restante = objetivo − progreso. Cumplida cuando progreso ≥ objetivo. Vencida cuando pasa la fecha sin cumplirse.
- Las cuentas pueden quedar en negativo (no se bloquea por saldo insuficiente).

## Requisitos funcionales (EARS)
- El sistema DEBE permitir crear una meta (nombre, objetivo > 0, fecha objetivo futura obligatoria).
- El sistema DEBE permitir registrar aportes (cuenta propia, monto > 0, categoría de gasto predefinida o propia) que crean un gasto vinculado y descuentan la cuenta.
- El sistema DEBE mostrar progreso, restante, % de avance y estado (en curso / cumplida / vencida).
- El sistema DEBE permitir editar (nombre, objetivo, fecha futura) y eliminar metas propias. Al eliminar una meta, sus movimientos de aporte se conservan como gastos normales (se desvinculan).
- El sistema DEBE devolver 401 sin autenticación y 404 ante meta/cuenta ajena o inexistente.
- El sistema SOLO DEBE incluir datos del usuario autenticado.

## Requisitos no funcionales
- `decimal` en backend; UI en español, responsive; nueva migración (tablas nuevas).

## Casos límite
- Objetivo ≤ 0 o aporte ≤ 0 → 400.
- Fecha objetivo pasada al crear o editar → 400.
- Aporte que supera el objetivo → se acepta (meta cumplida, muestra excedente).
- Cuenta ajena en el aporte → 404; categoría inexistente, de tipo Income o personalizada ajena → 400.

## Fuera de alcance
- Retiros de la meta (solo aportar; eliminar la meta desvincula sus movimientos).
- Recordatorios/notificaciones.

## Criterios de finalización
- CRUD + aportes funcionan; restante y % correctos; cumplida visible; solo datos propios; responsive.

## Dudas abiertas
- (resueltas: fecha objetivo obligatoria y futura; aportes vinculados a cuenta como gastos con categoría a elección; estado Vencida; eliminar meta desvincula movimientos)

## Estado
- Spec actualizada, pendiente re-planificación del plan 010.
