# 🍔 Guía de Uso - Sistema de Inventario para Restaurante de Comidas Rápidas

## ✅ Estado Actual del Sistema

### Servicios Activos

- **Backend API**: http://localhost:5261
- **Frontend React**: http://localhost:3000
- **Base de Datos**: SQLite (local)

### Datos Iniciales Cargados

#### 📂 Categorías (5)

1. **Carnes** - Carnes para hamburguesas, pollo, etc.
2. **Vegetales** - Lechugas, tomates, cebollas, etc.
3. **Panes** - Panes para hamburguesas y hot dogs
4. **Salsas y Aderezos** - Ketchup, mayonesa, mostaza, etc.
5. **Bebidas** - Gaseosas, jugos, etc.

#### 🥘 Ingredientes Creados (10)

1. Carne de Res para Hamburguesa (10-50 kg)
2. Pechuga de Pollo (8-40 kg)
3. Tomate (5-20 kg)
4. Lechuga (3-15 kg)
5. Cebolla (4-20 kg)
6. Pan para Hot Dog (30-150 unidades)
7. Mayonesa (2-10 L)
8. Ketchup (2-10 L)
9. Mostaza (1-5 L)
10. Coca-Cola (20-100 unidades)

---

## 🚀 Cómo Usar el Sistema

### 1. Gestión de Ingredientes

#### ➕ Crear Nuevo Ingrediente

1. Ve a la sección **"Ingredientes"** en el menú lateral
2. Haz clic en el botón **"+ Nuevo Ingrediente"**
3. Completa el formulario:
   - **Nombre**: Nombre del ingrediente (ej: "Tocineta")
   - **Descripción**: Descripción detallada
   - **Categoría**: Selecciona una categoría existente
   - **Unidad de Medida**: kg, g, L, ml o unidad
   - **Stock Mínimo**: Cantidad mínima antes de reabastecer
   - **Stock Máximo**: Cantidad máxima recomendada
4. Haz clic en **"Crear Ingrediente"**

#### 🔍 Buscar Ingredientes

- Usa la barra de búsqueda en la parte superior
- Filtra por nombre de ingrediente
- Ve el estado del stock en tiempo real:
  - 🟢 **Normal**: Stock entre mínimo y máximo
  - 🔴 **Bajo Stock**: Stock por debajo del mínimo
  - 🟡 **Exceso**: Stock por encima del máximo

### 2. Gestión de Categorías

1. Ve a **"Categorías"** en el menú
2. Visualiza todas las categorías existentes
3. Crea nuevas categorías según necesites

### 3. Gestión de Proveedores

1. Accede a **"Proveedores"**
2. Registra nuevos proveedores con:
   - Nombre comercial
   - RUT/NIT
   - Contacto
   - Dirección

### 4. Órdenes de Compra

#### Crear una Orden de Compra

1. Ve a **"Órdenes de Compra"**
2. Crea una nueva orden indicando:
   - Proveedor
   - Ingredientes y cantidades
   - Fecha esperada de entrega
3. Estados de órdenes:
   - **Pendiente**: Orden creada, esperando aprobación
   - **Aprobada**: Orden aprobada, esperando entrega
   - **Recibida**: Mercancía recibida e ingresada al inventario

### 5. Dashboard

El dashboard muestra:

- 📊 Total de ingredientes
- ⚠️ Ingredientes con stock bajo
- 📦 Lotes próximos a vencer
- 🛒 Órdenes pendientes de aprobación

---

## 🔧 Funcionalidades Implementadas

### ✅ Funcionalidades Activas

- [x] Crear ingredientes con categorías
- [x] Visualizar inventario completo
- [x] Buscar y filtrar ingredientes
- [x] Alertas de stock bajo/exceso
- [x] Gestión de categorías
- [x] API REST completa
- [x] Modal de creación de ingredientes

### 🚧 Próximas Funcionalidades

- [ ] Editar ingredientes
- [ ] Eliminar ingredientes
- [ ] Gestión completa de órdenes de compra
- [ ] Recepción de lotes
- [ ] Registro de consumo
- [ ] Historial de movimientos
- [ ] Reportes y estadísticas

---

## 🐛 Solución de Problemas

### El frontend no carga

```bash
cd frontend
npm install
npm start
```

### El backend no responde

```bash
cd backend
dotnet run --project InventarioDDD.API
```

### Reiniciar ambos servicios

```bash
.\start.bat
```

### Ver logs del backend

- Los logs se muestran en la consola del backend
- Revisa la terminal "Backend API"

### Base de datos

- Ubicación: `backend/inventario.db`
- Para resetear: elimina el archivo y reinicia el backend

---

## 📚 Arquitectura DDD

El proyecto sigue los principios de **Domain-Driven Design**:

### Capas

1. **Domain**: Entidades, Value Objects, Agregados, Eventos
2. **Application**: Comandos, Queries, Handlers (CQRS + MediatR)
3. **Infrastructure**: Repositorios, Persistencia (EF Core + SQLite)
4. **API**: Controladores REST, Middleware

### Patrones Implementados

- ✅ Aggregates (IngredienteAggregate, OrdenDeCompraAggregate)
- ✅ Value Objects (UnidadDeMedida, RangoDeStock, Cantidad)
- ✅ Domain Events
- ✅ Repository Pattern
- ✅ CQRS (Command Query Responsibility Segregation)
- ✅ Mediator Pattern

---

## 🎯 Casos de Uso Típicos

### Caso 1: Stock Bajo - Crear Orden de Compra

1. Dashboard muestra ingredientes con stock bajo
2. Crear orden de compra para esos ingredientes
3. Aprobar la orden
4. Recibir mercancía
5. El sistema actualiza automáticamente el inventario

### Caso 2: Registrar Consumo Diario

1. Al final del día, registrar consumo de ingredientes
2. El sistema reduce el stock automáticamente
3. Genera alertas si algún ingrediente queda bajo

### Caso 3: Control de Vencimientos

1. Sistema alerta sobre lotes próximos a vencer
2. Priorizar uso de esos lotes
3. Evitar pérdidas por vencimiento

---

## 💡 Tips

- **Stock Mínimo**: Configúralo considerando el tiempo de entrega del proveedor
- **Stock Máximo**: Basado en capacidad de almacenamiento y rotación
- **Unidades**: Usa la unidad más práctica para tu cocina
- **Categorías**: Organiza por tipo de producto o ubicación en la cocina

---

## 📞 Soporte

Para más información, consulta:

- `docs/README.md` - Documentación técnica completa
- `docs/ESTRUCTURA_CARPETAS_DDD.md` - Estructura del proyecto
- Swagger UI: http://localhost:5261/swagger

---

**¡El sistema está listo para usar! 🎉**
