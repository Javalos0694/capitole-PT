# ADR 001 — Validación

## Contexto
Hay reglas en value objects, commands y formulario. Hay que evitar duplicar de forma inconsistente.

## Decisión
- Dominio: auto-validación en VOs (`Email`, `Customer`, `DateRange`) y métodos del agregado.
- Application: FluentValidation sobre commands (entrada de API).
- Front: Zod + React Hook Form, alineado con las reglas básicas; el servidor manda.

## Alternativas
Validar solo en API o solo en dominio.

## Consecuencias
Errores tempranos y códigos estables (`booking.*`) sin contaminar el dominio con FluentValidation.
