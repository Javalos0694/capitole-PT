# ADR 003 — Solape y concurrencia

## Contexto
Dos confirmaciones solapadas del mismo recurso no pueden quedar ambas Confirmed.

## Decisión
- Regla de solape como `ConfirmedOverlappingBookingSpec` traducible a EF.
- Intervalo semiabierto `[Start, End)`.
- Token `xmin` de PostgreSQL como concurrencia optimista del agregado.

## Alternativas
Exclusion constraint GiST, advisory locks o isolation serializable.

## Consecuencias
El check en aplicación cubre el caso feliz. Para carrera dura entre dos filas distintas, el siguiente paso natural sería un `EXCLUDE` en PostgreSQL.
