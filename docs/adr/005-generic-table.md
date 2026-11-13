# ADR 005 — GenericTable (React)

## Contexto
Se pide una tabla genérica desacoplada del dominio.

## Decisión
Componente `GenericTable<T>` con `header`, `renderRow`, `getRowKey` y soporte de teclado (Enter/Espacio). Tests con Testing Library.

## Alternativas
Tabla acoplada a Booking o librería externa (TanStack Table).

## Consecuencias
API mínima y testeable. El listado de reservas solo compone la tabla.
