# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-10-07

### Added

#### Domain Layer
- Value Objects: `CantidadDisponible`, `UnidadDeMedida`, `FechaVencimiento`, `Categoria`, `RangoDeStock`, `PrecioConMoneda`
- Entities: `Lote`, `Proveedor`, `OrdenDeCompra`, `Pedido`
- Aggregate Root: `Ingrediente` with full FIFO logic and invariants
- 10 Domain Events with MediatR
- 6 Domain Services interfaces

#### Application Layer
- CQRS pattern with MediatR
- 11 Use Cases (Commands and Queries)
- 10 Event Handlers for domain events
- DTOs for data transfer

#### Infrastructure Layer
- 3 InMemory repositories (thread-safe)
- 6 Domain Services implementations:
  - `ServicioDeInventario` - Stock management
  - `ServicioDeConsumo` - FIFO consumption
  - `ServicioDeReabastecimiento` - Automatic reordering
  - `ServicioDeRecepcion` - Quality control
  - `ServicioDeAuditoria` - Reports and auditing
  - `ServicioDeRotacion` - Rotation analysis

#### API Layer
- 6 REST Controllers with 34 endpoints total
- Swagger/OpenAPI documentation
- 4 Scheduled Jobs with Quartz.NET:
  - Stock alerts (every 5 minutes)
  - Expired batches detection (hourly)
  - Expiration warnings (daily at 8 AM)
  - Cache cleanup (daily at midnight)

### Features
- ✅ Clean Architecture with DDD
- ✅ Event-Driven Architecture
- ✅ CQRS Pattern
- ✅ Repository Pattern
- ✅ Aggregate Pattern
- ✅ FIFO automatic consumption
- ✅ Automatic purchase orders
- ✅ Quality control on reception
- ✅ Scheduled alerts system
- ✅ Audit reports
- ✅ Rotation analysis
- ✅ Complete REST API

### Technologies
- .NET 9.0
- C# 12
- ASP.NET Core
- MediatR 13.0.0
- Quartz.NET 3.15.0
- Swashbuckle.AspNetCore 9.0.6

[1.0.0]: https://github.com/daniellelooo/ddd-inventario/releases/tag/v1.0.0
