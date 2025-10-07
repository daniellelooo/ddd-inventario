# 📘 Guía de Uso - Swagger UI

## 🚀 Acceso Rápido

1. **Ejecutar la aplicación:**
   ```bash
   cd InventarioDDD.API
   dotnet run
   ```

2. **Abrir Swagger UI:**
   - **URL Principal**: http://localhost:5261
   - **URL Alternativa**: http://localhost:5261/swagger

## 🎯 Navegación en Swagger UI

### Grupos de Endpoints

La API está organizada en **6 grupos principales**:

#### 1️⃣ **Ingredientes** (`/api/ingredientes`)
Gestión completa de ingredientes con control de stock y lotes.

**Endpoints disponibles:**
- `GET /api/ingredientes` - Listar todos los ingredientes
- `GET /api/ingredientes/{id}` - Obtener ingrediente específico
- `POST /api/ingredientes` - Registrar nuevo ingrediente
- `POST /api/ingredientes/{id}/lotes` - Agregar lote a ingrediente

#### 2️⃣ **Proveedores** (`/api/proveedores`)
Administración de proveedores y su estado.

**Endpoints disponibles:**
- `GET /api/proveedores` - Listar todos
- `GET /api/proveedores/{id}` - Obtener por ID
- `POST /api/proveedores` - Registrar nuevo proveedor
- `PUT /api/proveedores/{id}` - Actualizar información
- `PATCH /api/proveedores/{id}/estado` - Activar/Desactivar

#### 3️⃣ **Órdenes de Compra** (`/api/ordenesdecompra`)
Control de órdenes con máquina de estados.

**Endpoints disponibles:**
- `GET /api/ordenesdecompra` - Listar todas
- `GET /api/ordenesdecompra/{id}` - Obtener por ID
- `POST /api/ordenesdecompra` - Crear nueva orden
- `POST /api/ordenesdecompra/{id}/aprobar` - Aprobar orden
- `POST /api/ordenesdecompra/{id}/en-transito` - Marcar en tránsito
- `POST /api/ordenesdecompra/{id}/recibida` - Marcar recibida
- `POST /api/ordenesdecompra/{id}/cancelar` - Cancelar orden
- `GET /api/ordenesdecompra/por-estado/{estado}` - Filtrar por estado

#### 4️⃣ **Inventario** (`/api/inventario`)
Operaciones de stock, consumo FIFO y reabastecimiento.

**Endpoints disponibles:**
- `GET /api/inventario/stock/{id}` - Consultar stock disponible
- `GET /api/inventario/lotes/{id}` - Ver todos los lotes
- `POST /api/inventario/consumir` - Consumir ingredientes (FIFO automático)
- `POST /api/inventario/validar-stock` - Validar disponibilidad
- `POST /api/inventario/generar-ordenes-automaticas` - Generar órdenes automáticas
- `GET /api/inventario/punto-reorden/{id}` - Calcular punto de reorden
- `GET /api/inventario/sugerir-proveedor/{id}` - Sugerir mejor proveedor

#### 5️⃣ **Auditoría** (`/api/auditoria`)
Reportes y análisis de consumo/mermas.

**Endpoints disponibles:**
- `GET /api/auditoria/reporte-consumo` - Reporte de consumo por período
- `GET /api/auditoria/reporte-mermas` - Reporte de desperdicios
- `POST /api/auditoria/comparar-inventario` - Comparar físico vs sistema
- `GET /api/auditoria/valoracion-inventario` - Valoración total

#### 6️⃣ **Rotación** (`/api/rotacion`)
Análisis de rotación y proyecciones.

**Endpoints disponibles:**
- `GET /api/rotacion/ingrediente/{id}/indicador` - Indicador de rotación
- `GET /api/rotacion/lenta-rotacion` - Productos lentos (>60 días)
- `GET /api/rotacion/alta-rotacion` - Productos rápidos (<15 días)
- `GET /api/rotacion/proyectar-demanda/{id}` - Proyección de demanda
- `GET /api/rotacion/consumo-promedio/{id}` - Consumo diario promedio
- `GET /api/rotacion/tiempo-promedio/{id}` - Tiempo promedio en inventario

## 🎬 Flujo de Trabajo Recomendado

### Escenario 1: Configuración Inicial

1. **Registrar Proveedores** (`POST /api/proveedores`)
   ```json
   {
     "nombre": "Proveedor ABC",
     "contacto": "Juan Pérez",
     "telefono": "+123456789",
     "email": "contacto@proveedorabc.com",
     "direccion": "Calle Principal 123"
   }
   ```

2. **Registrar Ingredientes** (`POST /api/ingredientes`)
   ```json
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

3. **Agregar Lotes** (`POST /api/ingredientes/{id}/lotes`)
   ```json
   {
     "cantidad": 25.0,
     "fechaVencimiento": "2025-10-15",
     "precioUnitario": 2.50,
     "proveedorId": 1
   }
   ```

### Escenario 2: Operación Diaria

1. **Consultar Stock** (`GET /api/inventario/stock/{id}`)
   - Ver disponibilidad actual

2. **Consumir Ingredientes** (`POST /api/inventario/consumir`)
   ```json
   {
     "pedidoId": 123,
     "ingredientesIds": [1, 2, 3]
   }
   ```
   - FIFO automático: consume primero los lotes más antiguos

3. **Verificar Alertas en Logs**
   - Los schedulers detectan automáticamente:
     - Stock bajo (cada 5 min)
     - Lotes vencidos (cada hora)
     - Próximos vencimientos (diario 8 AM)

### Escenario 3: Reabastecimiento

1. **Verificar Punto de Reorden** (`GET /api/inventario/punto-reorden/{id}`)
   - Calcula: Mínimo + (Consumo Diario × Días de Entrega)

2. **Generar Órdenes Automáticas** (`POST /api/inventario/generar-ordenes-automaticas`)
   - Crea órdenes para todos los ingredientes que necesitan reabastecimiento

3. **Aprobar Orden** (`POST /api/ordenesdecompra/{id}/aprobar`)

4. **Marcar en Tránsito** (`POST /api/ordenesdecompra/{id}/en-transito`)

5. **Registrar Recepción** (`POST /api/ordenesdecompra/{id}/recibida`)

### Escenario 4: Reportes y Análisis

1. **Reporte de Consumo** (`GET /api/auditoria/reporte-consumo`)
   ```
   Query params: ?fechaInicio=2025-10-01&fechaFin=2025-10-07
   ```

2. **Análisis de Rotación** (`GET /api/rotacion/lenta-rotacion`)
   - Identifica productos que no se mueven

3. **Proyección de Demanda** (`GET /api/rotacion/proyectar-demanda/{id}`)
   ```
   Query params: ?diasProyeccion=30
   ```

## 💡 Tips de Uso en Swagger UI

### Probar un Endpoint

1. **Click en el endpoint** para expandirlo
2. **Click "Try it out"** en la esquina derecha
3. **Editar parámetros/body** según necesites
4. **Click "Execute"** para enviar la petición
5. **Ver respuesta** más abajo con código de estado y datos

### Schemas/Modelos

- Click en **"Schemas"** al final de la página
- Ver todos los modelos de datos disponibles
- Útil para entender estructura de requests/responses

### Filtros

- Usa el **campo de búsqueda** arriba para filtrar endpoints
- Busca por: "stock", "orden", "lote", etc.

### Exportar/Importar

- **Download OpenAPI spec**: Click en `/swagger/v1/swagger.json`
- Usa con herramientas como Postman, Insomnia, etc.

## 🔥 Ejemplos Prácticos

### Ejemplo 1: Crear y Consumir Ingrediente

```bash
# 1. Crear ingrediente
POST /api/ingredientes
{
  "nombre": "Lechuga",
  "categoria": "Verdura",
  "unidadDeMedida": "kg",
  "stockMinimo": 5,
  "stockOptimo": 20,
  "stockMaximo": 50,
  "diasVidaUtil": 5
}
# Response: { "id": 1, ... }

# 2. Agregar lote
POST /api/ingredientes/1/lotes
{
  "cantidad": 15.0,
  "fechaVencimiento": "2025-10-12",
  "precioUnitario": 3.00,
  "proveedorId": 1
}

# 3. Consultar stock
GET /api/inventario/stock/1
# Response: { "stockDisponible": 15.0, "unidadMedida": "kg" }

# 4. Consumir (FIFO)
POST /api/inventario/consumir
{
  "pedidoId": 1,
  "ingredientesIds": [1]
}
```

### Ejemplo 2: Ciclo de Orden de Compra

```bash
# 1. Crear orden
POST /api/ordenesdecompra
{
  "proveedorId": 1,
  "fechaEntregaEsperada": "2025-10-15",
  "items": [
    {
      "ingredienteId": 1,
      "cantidad": 50.0,
      "precioUnitario": 2.80
    }
  ]
}
# Response: { "id": 1, "estado": "Pendiente" }

# 2. Aprobar
POST /api/ordenesdecompra/1/aprobar

# 3. En tránsito
POST /api/ordenesdecompra/1/en-transito

# 4. Recibida
POST /api/ordenesdecompra/1/recibida
```

## 📊 Monitoreo de Jobs Automáticos

Los schedulers ejecutan tareas en background. Ver logs en la consola:

```
info: AlertasStockBajoJob - Ejecutando...
info: LotesVencidosJob - Ejecutando...
warn: ⚠️ ALERTA: 2 ingredientes con stock bajo
```

## 🆘 Solución de Problemas

### Error 400 (Bad Request)
- **Causa**: Datos inválidos en el body
- **Solución**: Verifica el schema del modelo en Swagger

### Error 404 (Not Found)
- **Causa**: ID no existe
- **Solución**: Lista todos los recursos primero (GET)

### Error 500 (Internal Server Error)
- **Causa**: Error en la lógica de negocio
- **Solución**: Revisa los logs en la consola

## 📚 Recursos Adicionales

- **OpenAPI Spec**: http://localhost:5261/swagger/v1/swagger.json
- **API Info**: http://localhost:5261/api
- **README**: Ver archivo README.md en la raíz del proyecto

## 🎓 Arquitectura DDD en Acción

Cuando usas la API, estás interactuando con:

- **Value Objects**: `CantidadDisponible`, `FechaVencimiento`, etc.
- **Entities**: `Proveedor`, `OrdenDeCompra`
- **Aggregates**: `Ingrediente` (raíz que controla `Lote`)
- **Domain Events**: Se publican automáticamente
- **Domain Services**: Orquestan lógica compleja
- **CQRS**: Commands (POST/PUT) y Queries (GET) separados

¡Disfruta explorando la API! 🚀
