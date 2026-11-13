# ADR 004 — Outbox e idempotencia

## Contexto
Confirmar/cancelar debe publicar a RabbitMQ sin doble escritura frágil.

## Decisión
Transactional Outbox propia: el handler de dominio escribe `OutboxMessages` en la misma UoW. Un `BackgroundService` publica con MassTransit. El worker usa inbox por `EventId`.

## Alternativas
Outbox de MassTransit/EF o publicar directo en el handler.

## Consecuencias
CA10 se cumple: si RabbitMQ cae, el commit de negocio sigue; al volver, el publisher reintenta.
