# ADR 002 — Mediator

## Contexto
CQRS pide un mediator con pipeline (validation + logging).

## Decisión
MediatR 12. Maduro, behaviors simples y suficiente para el tamaño del MVP.

## Alternativas
Mediator source-generator (más rendimiento, menos ecosistema) o bus propio.

## Consecuencias
Dependencia extra y licencia Apache-2.0. Pipeline homogéneo para todos los use cases.
