# 📂 Guía de Estructura DDD - Sistema de Inventario

## 🎯 Introducción

Esta guía explica la estructura de carpetas del backend siguiendo los principios de **Domain-Driven Design (DDD)** y **Clean Architecture**. El proyecto está organizado en 4 capas principales, cada una con responsabilidades específicas.

---

## 🏗️ Estructura General de la Solución

El proyecto backend contiene 4 proyectos principales:

- **InventarioDDD.API** → Capa de Presentación (API REST)
- **InventarioDDD.Application** → Capa de Aplicación (CQRS)
- **InventarioDDD.Domain** → Capa de Dominio (Lógica de Negocio)
- **InventarioDDD.Infrastructure** → Capa de Infraestructura (Persistencia)

---

## 1️⃣ Domain Layer - `InventarioDDD.Domain/`

**La capa más importante** - Contiene toda la lógica de negocio pura, sin dependencias externas.

### 📁 Carpetas Principales:

#### **Aggregates/**
**Qué es:** Raíces de Agregados - Agrupan entidades relacionadas y protegen la consistencia del grupo.

**Contiene:**
- `IngredienteAggregate.cs` - Agrupa ingrediente con sus lotes relacionados
- `ProveedorAggregate.cs` - Agrupa proveedor con sus órdenes de compra
- `OrdenDeCompraAggregate.cs` - Agrupa orden con sus detalles y validaciones

**Propósito:**
- Encapsulan reglas de negocio complejas que involucran múltiples entidades
- Definen límites transaccionales (todo se guarda junto o nada)
- Coordinan operaciones entre entidades relacionadas
- Protegen la integridad de los datos del grupo

---

#### **Entities/**
**Qué es:** Entidades del Dominio - Objetos con identidad única que persisten en el tiempo.

**Contiene:**
- `Ingrediente.cs` - Productos que se gestionan en el inventario (harina, azúcar, etc.)
- `Proveedor.cs` - Empresas que suministran los ingredientes
- `Categoria.cs` - Clasificación de ingredientes (Carnes, Lácteos, Verduras, etc.)
- `Lote.cs` - Grupos de ingredientes con código, fecha de vencimiento y cantidad
- `OrdenDeCompra.cs` - Pedidos realizados a proveedores
- `MovimientoInventario.cs` - Registro histórico de entradas y salidas

**Características:**
- Todas tienen un identificador único (Guid)
- Pueden cambiar su estado a lo largo del tiempo
- Contienen lógica de negocio específica
- Se comparan por su identidad, no por sus valores

---

#### **ValueObjects/**
**Qué es:** Objetos de Valor - Conceptos del negocio sin identidad, definidos únicamente por sus valores.

**Contiene:**
- `UnidadDeMedida.cs` - Unidades de medida (kilogramos, litros, unidades, libras, onzas)
- `RangoDeStock.cs` - Define stock mínimo y máximo permitido
- `PrecioConMoneda.cs` - Precio con su moneda asociada (evita mezclar COP con USD)
- `CantidadInventario.cs` - Cantidad con validaciones (no negativos)
- `FechaVencimiento.cs` - Fecha con lógica para verificar vencimiento
- `DireccionProveedor.cs` - Dirección completa estructurada (calle, ciudad, país, código postal)

**Características:**
- NO tienen identificador único
- Son inmutables (no cambian después de crearse)
- Se comparan por sus valores, no por identidad
- Contienen validaciones del negocio
- Pueden tener objetos predefinidos comunes

---

#### **Enums/**
**Qué es:** Enumeraciones - Valores fijos y predefinidos del dominio.

**Contiene:**
- `EstadoOrden.cs` - Estados del ciclo de vida de una orden (Pendiente, Aprobada, EnvioPendiente, Recibida, Cancelada)
- `TipoMovimiento.cs` - Tipos de movimientos de inventario (Entrada, Salida, Ajuste)
- `Moneda.cs` - Monedas soportadas (COP, USD, EUR)

**Propósito:**
- Definir valores constantes del dominio
- Evitar usar strings o números mágicos
- Facilitar validaciones y comparaciones

---

#### **Interfaces/**
**Qué es:** Contratos de Repositorios - Definen cómo se accede a los datos sin depender de la implementación.

**Contiene:**
- `IRepositorios.cs` - Archivo único con todas las interfaces de repositorios

**Repositorios definidos:**
- `IIngredienteRepository` - Acceso a datos de ingredientes
- `IProveedorRepository` - Acceso a datos de proveedores
- `ICategoriaRepository` - Acceso a datos de categorías
- `ILoteRepository` - Acceso a datos de lotes
- `IOrdenDeCompraRepository` - Acceso a datos de órdenes de compra
- `IMovimientoInventarioRepository` - Acceso a datos de movimientos

**Propósito:**
- Abstraer la persistencia de datos
- Permitir cambiar la implementación sin afectar el dominio
- Facilitar testing con mocks

---

#### **Events/**
**Qué es:** Eventos del Dominio - Representan cosas que han ocurrido en el negocio.

**Estado actual:** Vacío (preparado para futura implementación)

**Ejemplos de eventos futuros:**
- `IngredienteCreado` - Se creó un nuevo ingrediente
- `StockBajo` - El stock cayó por debajo del mínimo
- `OrdenAprobada` - Una orden fue aprobada
- `LoteVencido` - Un lote alcanzó su fecha de vencimiento

---

#### **Services/**
**Qué es:** Servicios de Dominio - Lógica de negocio que no pertenece a ninguna entidad específica.

**Estado actual:** Vacío (la lógica está distribuida en agregados por ahora)

**Ejemplos de servicios futuros:**
- Cálculo de cantidades sugeridas de reabastecimiento
- Validaciones complejas entre múltiples agregados
- Políticas de precios

---

## 2️⃣ Application Layer - `InventarioDDD.Application/`

**Capa de Aplicación** - Coordina las operaciones usando el patrón CQRS (Command Query Responsibility Segregation).

### 📁 Carpetas Principales:

#### **Commands/**
**Qué es:** Comandos de Escritura - Representan intenciones de cambiar el estado del sistema.

**Contiene:**
- `CrearOrdenDeCompraCommand.cs` - Crear nueva orden de compra a un proveedor
- `AprobarOrdenDeCompraCommand.cs` - Aprobar una orden pendiente
- `RecibirOrdenDeCompraCommand.cs` - Registrar recepción de orden y crear lote
- `RegistrarConsumoCommand.cs` - Consumir ingredientes del inventario
- `CrearCategoriaCommand.cs` - Crear nueva categoría de ingredientes
- `CrearProveedorCommand.cs` - Crear nuevo proveedor
- `CrearIngredienteCommand.cs` - Crear nuevo ingrediente

**Características:**
- Representan acciones (verbos: Crear, Aprobar, Registrar, Actualizar)
- Modifican el estado del sistema
- Retornan resultados simples (Id creado, booleano, void)
- Se procesan mediante handlers

---

#### **Queries/**
**Qué es:** Consultas de Lectura - Solicitan datos sin modificar el estado.

**Contiene:**
- `ObtenerOrdenesPendientesQuery.cs` - Listar órdenes de compra pendientes
- `ObtenerLotesProximosAVencerQuery.cs` - Lotes que están por vencer
- `ObtenerIngredientesParaReabastecerQuery.cs` - Ingredientes con stock bajo
- `ObtenerHistorialMovimientosQuery.cs` - Historial de movimientos de inventario

**Características:**
- Representan preguntas (sustantivos: Obtener, Listar, Buscar)
- NO modifican el estado del sistema
- Retornan DTOs o listas de datos
- Optimizadas para lectura

---

#### **Handlers/**
**Qué es:** Manejadores - Contienen la lógica para ejecutar comandos y consultas.

**Contiene:**
- Un handler por cada comando y query
- `CrearOrdenDeCompraHandler.cs` - Procesa la creación de órdenes
- `AprobarOrdenDeCompraHandler.cs` - Procesa aprobación de órdenes
- `RecibirOrdenDeCompraHandler.cs` - Procesa recepción de órdenes
- `RegistrarConsumoHandler.cs` - Procesa consumo de ingredientes
- `CrearCategoriaHandler.cs` - Procesa creación de categorías
- `CrearProveedorHandler.cs` - Procesa creación de proveedores
- `CrearIngredienteHandler.cs` - Procesa creación de ingredientes
- `ObtenerOrdenesPendientesHandler.cs` - Obtiene órdenes pendientes
- `ObtenerLotesProximosAVencerHandler.cs` - Obtiene lotes por vencer
- `ObtenerIngredientesParaReabastecerHandler.cs` - Obtiene ingredientes a reabastecer
- `ObtenerHistorialMovimientosHandler.cs` - Obtiene historial de movimientos

**Responsabilidades:**
- Validar datos de entrada
- Validar reglas de negocio
- Llamar a repositorios para persistencia o lectura
- Crear y coordinar entidades del dominio
- Transformar entre DTOs y entidades del dominio
- Manejar transacciones

---

#### **DTOs/**
**Qué es:** Data Transfer Objects - Objetos simples para transferir datos entre capas.

**Contiene:**
- `IngredienteDto.cs` - Datos de ingredientes para mostrar
- `LoteDto.cs` - Datos de lotes para mostrar
- `OrdenDeCompraDto.cs` - Datos de órdenes de compra para mostrar
- `MovimientoInventarioDto.cs` - Datos de movimientos para mostrar

**Características:**
- Son clases simples con solo propiedades
- NO tienen lógica de negocio
- Fáciles de serializar a JSON
- Usados para comunicación con la capa de presentación
- Evitan exponer las entidades del dominio directamente

---

## 3️⃣ Infrastructure Layer - `InventarioDDD.Infrastructure/`

**Capa de Infraestructura** - Implementa los detalles técnicos (base de datos, servicios externos).

### 📁 Carpetas Principales:

#### **Persistence/**
**Qué es:** Persistencia de Datos - Configuración de Entity Framework Core y acceso a la base de datos.

**Contiene:**
- `InventarioDbContext.cs` - Contexto principal de Entity Framework que representa la base de datos

**Responsabilidades del DbContext:**
- Define las tablas (DbSets) del sistema
- Configura la conexión a la base de datos SQLite
- Aplica todas las configuraciones de entidades
- Maneja transacciones y guardado de cambios

**Tablas que maneja:**
- Ingredientes
- Proveedores
- Categorias
- Lotes
- OrdenesCompra
- MovimientosInventario

---

#### **Configuration/**
**Qué es:** Configuraciones de Entity Framework - Define cómo se mapean las entidades a tablas.

**Contiene:**
- `IngredienteConfiguration.cs` - Mapeo de Ingrediente a tabla y columnas
- `ProveedorConfiguration.cs` - Mapeo de Proveedor a tabla y columnas
- `CategoriaConfiguration.cs` - Mapeo de Categoria a tabla y columnas
- `LoteConfiguration.cs` - Mapeo de Lote a tabla y columnas
- `OrdenDeCompraConfiguration.cs` - Mapeo de OrdenDeCompra a tabla y columnas
- `MovimientoInventarioConfiguration.cs` - Mapeo de MovimientoInventario a tabla y columnas

**Responsabilidades:**
- Definir nombre de tabla
- Configurar clave primaria
- Mapear Value Objects a columnas (OwnsOne)
- Definir conversiones de tipos personalizados
- Crear índices únicos para optimización
- Configurar relaciones entre tablas (Foreign Keys)
- Definir restricciones de base de datos

---

#### **Repositories/**
**Qué es:** Implementación de Repositorios - Implementan las interfaces definidas en la capa de dominio.

**Contiene:**
- `IngredienteRepository.cs` - Implementa IIngredienteRepository
- `ProveedorRepository.cs` - Implementa IProveedorRepository
- `CategoriaRepository.cs` - Implementa ICategoriaRepository
- `LoteRepository.cs` - Implementa ILoteRepository
- `OrdenDeCompraRepository.cs` - Implementa IOrdenDeCompraRepository
- `MovimientoInventarioRepository.cs` - Implementa IMovimientoInventarioRepository

**Responsabilidades:**
- Ejecutar consultas a la base de datos usando Entity Framework
- Convertir registros de base de datos a Agregados del dominio
- Guardar Agregados del dominio en la base de datos
- Reconstruir objetos completos con sus relaciones
- Ocultar detalles técnicos de persistencia al dominio
- Manejar operaciones CRUD (Create, Read, Update, Delete)

---

#### **Cache/**
**Qué es:** Caché - Almacenamiento temporal de datos para mejorar rendimiento.

**Estado actual:** Vacío (preparado para futura implementación)

**Uso futuro:**
- Cachear ingredientes consultados frecuentemente
- Cachear categorías (cambian poco)
- Cachear proveedores activos
- Reducir consultas repetitivas a la base de datos

---

## 4️⃣ API Layer - `InventarioDDD.API/`

**Capa de Presentación** - Expone la funcionalidad como API REST para el frontend.

### 📁 Carpetas Principales:

#### **Controllers/**
**Qué es:** Controladores REST - Exponen endpoints HTTP para que el frontend consuma.

**Contiene:**
- `IngredientesController.cs` - Endpoints para ingredientes (GET, POST)
- `ProveedoresController.cs` - Endpoints para proveedores (GET, POST)
- `CategoriasController.cs` - Endpoints para categorías (GET, POST)
- `LotesController.cs` - Endpoints para consultar lotes (GET)
- `OrdenesCompraController.cs` - Endpoints para órdenes de compra (GET, POST, PUT)
- `InventarioController.cs` - Endpoints para operaciones de inventario (POST consumo, GET historial)

**Responsabilidades:**
- Recibir peticiones HTTP del frontend
- Validar datos de entrada básicos
- Convertir requests HTTP a comandos/queries
- Enviar comandos/queries al Application Layer usando MediatR
- Retornar respuestas HTTP (200 OK, 201 Created, 400 Bad Request, etc.)
- Manejar serialización JSON

**Endpoints típicos:**
- GET /api/ingredientes - Listar todos
- POST /api/ingredientes - Crear nuevo
- GET /api/ordenescompra/pendientes - Órdenes pendientes
- POST /api/ordenescompra/{id}/aprobar - Aprobar orden
- POST /api/inventario/consumo - Registrar consumo

---

#### **Middleware/**
**Qué es:** Middleware - Interceptan requests y responses para agregar funcionalidad transversal.

**Contiene:**
- `ExceptionHandlingMiddleware.cs` - Manejo global de excepciones y errores

**Responsabilidades:**
- Capturar excepciones no manejadas
- Convertir excepciones a respuestas HTTP apropiadas
- Registrar errores en logs
- Devolver mensajes de error amigables al cliente
- Evitar que excepciones internas se expongan al frontend

---

#### **Properties/**
**Qué es:** Configuración de propiedades del proyecto.

**Contiene:**
- `launchSettings.json` - Configuración de cómo se ejecuta la aplicación

**Configura:**
- Puertos (5261 para HTTP)
- URLs de escucha
- Variables de entorno
- Perfil de desarrollo

---

#### **Archivos Raíz Importantes:**

**Program.cs**
- Punto de entrada de toda la aplicación
- Configura servicios (DbContext, MediatR, Repositorios, CORS)
- Registra inyección de dependencias
- Configura middleware pipeline
- Inicializa la base de datos
- Arranca el servidor web

**appsettings.json**
- Configuración general de la aplicación
- Cadena de conexión a base de datos
- Configuración de logging
- Configuración de CORS

**appsettings.Development.json**
- Configuración específica para desarrollo
- Override de configuraciones de appsettings.json
- Logging más detallado
- Desactiva ciertas validaciones

**InventarioDDD.API.csproj**
- Archivo de proyecto .NET
- Define referencias a otros proyectos
- Especifica paquetes NuGet
- Configura compilación

**Inventario.db**
- Base de datos SQLite (se genera automáticamente)
- Contiene todas las tablas del sistema
- Se crea al ejecutar por primera vez

---

## 🔗 Flujo de Dependencias entre Capas

**API Layer** → depende de → **Application Layer** + **Infrastructure Layer**

**Application Layer** → depende de → **Domain Layer**

**Infrastructure Layer** → depende de → **Domain Layer**

**Domain Layer** → NO depende de nadie (núcleo independiente)

---

## 📝 Archivos Clave del Sistema

| Archivo | Ubicación | Qué hace |
|---------|-----------|----------|
| `InventarioDDD.sln` | `/backend/` | Solución que agrupa los 4 proyectos |
| `Program.cs` | `/API/` | Punto de entrada, configuración general |
| `InventarioDbContext.cs` | `/Infrastructure/Persistence/` | Representa la base de datos |
| `IRepositorios.cs` | `/Domain/Interfaces/` | Define contratos de acceso a datos |
| `appsettings.json` | `/API/` | Configuración de conexión y comportamiento |
| `Inventario.db` | `/API/` | Base de datos SQLite física |

---

## 📖 Convenciones de Nombres

**Carpetas:**
- Usar PascalCase: `ValueObjects/`, `Commands/`, `Handlers/`

**Archivos:**
- Usar PascalCase: `IngredienteAggregate.cs`, `CrearOrdenCommand.cs`
- Sufijos descriptivos para identificar tipo:
  - `*Aggregate.cs` → Agregados
  - `*Command.cs` → Comandos de escritura
  - `*Query.cs` → Consultas de lectura
  - `*Handler.cs` → Manejadores de comandos/queries
  - `*Dto.cs` → Objetos de transferencia
  - `*Repository.cs` → Repositorios
  - `*Configuration.cs` → Configuraciones de Entity Framework
  - `*Controller.cs` → Controladores de API

**Interfaces:**
- Siempre iniciar con `I`: `IIngredienteRepository`, `ICategoriaRepository`

---

**Última actualización:** Octubre 10, 2025  
**Versión:** 1.0  
**Sistema:** Inventario DDD
