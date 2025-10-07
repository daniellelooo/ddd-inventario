# 🍳 Sistema de Inventario DDD - Restaurant Management

Sistema de gestión de inventario para restaurantes desarrollado con **Domain-Driven Design (DDD)** y **Clean Architecture** en .NET 9.

## 🏗️ Arquitectura

```
InventarioDDD/
├── Domain/              # Lógica de negocio pura (Value Objects, Entities, Aggregates)
├── Application/         # Casos de uso, Handlers, DTOs
├── Infrastructure/      # Implementaciones (Repositories, Services)
└── API/                 # REST API con ASP.NET Core
```

### Capas

- **Domain**: Core del negocio sin dependencias externas
- **Application**: Orquestación con MediatR (CQRS + Event-Driven)
- **Infrastructure**: Repositorios InMemory y servicios de dominio
- **API**: Controllers REST + Schedulers con Quartz.NET

## 🚀 Inicio Rápido

### Requisitos Previos
- .NET 9.0 SDK

### Ejecutar la Aplicación

```bash
cd InventarioDDD.API
dotnet run
```

La aplicación estará disponible en:
- **API**: http://localhost:5261
- **Swagger UI**: http://localhost:5261/swagger

> 📘 **Ver [GUIA_SWAGGER.md](GUIA_SWAGGER.md)** para instrucciones detalladas de uso

## 📋 Características Principales

### Gestión de Inventario
- ✅ Registro de ingredientes con Value Objects inmutables
- ✅ Control de stock con rangos (mínimo, óptimo, máximo)
- ✅ Lotes con fechas de vencimiento y FIFO automático
- ✅ Alertas de stock bajo y próximos vencimientos

### Proveedores y Órdenes de Compra
- ✅ Gestión de proveedores (activar/desactivar)
- ✅ Órdenes de compra con máquina de estados
- ✅ Generación automática de órdenes según punto de reorden
- ✅ Control de calidad en recepción de mercancía

### Auditoría y Reportes
- ✅ Reportes de consumo por período
- ✅ Reportes de mermas (vencimientos/desperdicios)
- ✅ Comparación de inventario físico vs. sistema
- ✅ Valoración total del inventario

### Análisis de Rotación
- ✅ Indicadores de rotación por ingrediente
- ✅ Identificación de productos de lenta rotación (>60 días)
- ✅ Identificación de productos de alta rotación (<15 días)
- ✅ Proyección de demanda futura
- ✅ Tiempo promedio en inventario

### Tareas Automáticas (Quartz.NET)
- 🕐 **Cada 5 minutos**: Verificación de stock bajo
- 🕐 **Cada hora**: Detección de lotes vencidos
- 🕐 **Diario 8 AM**: Alertas de próximos vencimientos (7 días)
- 🕐 **Diario medianoche**: Limpieza de caché y mantenimiento

## 🎯 Endpoints REST

### Ingredientes
- `GET /api/ingredientes` - Listar todos
- `GET /api/ingredientes/{id}` - Obtener por ID
- `POST /api/ingredientes` - Registrar nuevo
- `POST /api/ingredientes/{id}/lotes` - Agregar lote

### Proveedores
- `GET /api/proveedores` - Listar todos
- `POST /api/proveedores` - Registrar nuevo
- `PUT /api/proveedores/{id}` - Actualizar
- `PATCH /api/proveedores/{id}/estado` - Activar/Desactivar

### Órdenes de Compra
- `GET /api/ordenesdecompra` - Listar todas
- `POST /api/ordenesdecompra` - Crear nueva
- `POST /api/ordenesdecompra/{id}/aprobar` - Aprobar
- `POST /api/ordenesdecompra/{id}/en-transito` - Marcar en tránsito
- `POST /api/ordenesdecompra/{id}/recibida` - Marcar recibida
- `POST /api/ordenesdecompra/{id}/cancelar` - Cancelar
- `GET /api/ordenesdecompra/por-estado/{estado}` - Filtrar por estado

### Inventario
- `GET /api/inventario/stock/{id}` - Consultar stock
- `GET /api/inventario/lotes/{id}` - Obtener lotes
- `POST /api/inventario/consumir` - Consumir ingredientes (FIFO)
- `POST /api/inventario/validar-stock` - Validar stock suficiente
- `POST /api/inventario/generar-ordenes-automaticas` - Generar órdenes automáticas
- `GET /api/inventario/punto-reorden/{id}` - Calcular punto de reorden

### Auditoría
- `GET /api/auditoria/reporte-consumo` - Reporte de consumo
- `GET /api/auditoria/reporte-mermas` - Reporte de mermas
- `POST /api/auditoria/comparar-inventario` - Comparar inventario físico
- `GET /api/auditoria/valoracion-inventario` - Valoración total

### Rotación
- `GET /api/rotacion/ingrediente/{id}/indicador` - Indicador de rotación
- `GET /api/rotacion/lenta-rotacion` - Productos de lenta rotación
- `GET /api/rotacion/alta-rotacion` - Productos de alta rotación
- `GET /api/rotacion/proyectar-demanda/{id}` - Proyección de demanda
- `GET /api/rotacion/consumo-promedio/{id}` - Consumo promedio diario

## 🧩 Patrones Implementados

### Domain-Driven Design
- **Value Objects**: Inmutables con validaciones de negocio
- **Entities**: Identidad única y ciclo de vida
- **Aggregates**: `Ingrediente` como raíz de agregado controlando `Lote`
- **Domain Events**: 10 eventos publicados con MediatR
- **Domain Services**: 6 servicios para lógica que cruza agregados

### Arquitectura
- **Clean Architecture**: Separación en capas con dependencias hacia adentro
- **CQRS**: Separación de Commands y Queries con MediatR
- **Event-Driven**: Eventos de dominio para comunicación desacoplada
- **Repository Pattern**: Abstracción de persistencia

### Lógica de Negocio
- **FIFO**: Consumo automático por fecha de vencimiento
- **Invariantes**: Validaciones en constructores y setters privados
- **Estado**: Máquinas de estado para órdenes de compra
- **Composición**: Agregados con colecciones privadas

## 📦 Tecnologías

- **.NET 9.0** - Framework
- **C# 12** - Lenguaje (records, pattern matching)
- **ASP.NET Core** - Web API
- **MediatR 13.0** - CQRS y Event-Driven
- **Quartz.NET 3.15** - Schedulers
- **Swashbuckle** - Swagger/OpenAPI
- **InMemory Repositories** - Persistencia en memoria

## 🔧 Desarrollo

### Compilar
```bash
dotnet build
```

### Ejecutar Tests (próximamente)
```bash
dotnet test
```

### Estructura de Proyectos

**Domain** - Independiente de todo
```csharp
ValueObjects/     # Objetos inmutables (CantidadDisponible, FechaVencimiento, etc.)
Entities/         # Entidades con identidad (Proveedor, OrdenDeCompra)
Aggregates/       # Agregados raíz (Ingrediente)
Events/           # Eventos de dominio
Services/         # Interfaces de servicios de dominio
Repositories/     # Interfaces de repositorios
```

**Application** - Orquestación
```csharp
UseCases/         # Commands y Queries con handlers
EventHandlers/    # Handlers de eventos de dominio
DTOs/             # Data Transfer Objects
```

**Infrastructure** - Implementaciones
```csharp
Repositories/     # Repositorios InMemory
Services/         # Servicios de dominio implementados
```

**API** - Presentación
```csharp
Controllers/      # REST Controllers
Jobs/             # Scheduled jobs con Quartz
```

## 📝 Ejemplos de Uso

### Registrar Ingrediente
```json
POST /api/ingredientes
{
  "nombre": "Tomate",
  "categoria": "Verdura",
  "unidadDeMedida": "kg",
  "stockMinimo": 10.0,
  "stockOptimo": 50.0,
  "stockMaximo": 100.0,
  "diasVidaUtil": 7
}
```

### Agregar Lote
```json
POST /api/ingredientes/1/lotes
{
  "cantidad": 25.0,
  "fechaVencimiento": "2025-10-15",
  "precioUnitario": 2.50,
  "proveedorId": 1
}
```

### Consumir Ingredientes (FIFO automático)
```json
POST /api/inventario/consumir
{
  "pedidoId": 123,
  "ingredientesIds": [1, 2, 3]
}
```

## 📚 Documentación Adicional

La documentación completa de la API está disponible en Swagger UI cuando la aplicación está ejecutándose:
- http://localhost:5261/swagger

## 👨‍💻 Autor

Desarrollado con Clean Architecture y Domain-Driven Design.

## 📄 Licencia

Este proyecto es de código abierto para fines educativos.
