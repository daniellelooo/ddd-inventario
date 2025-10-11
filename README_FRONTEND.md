# ⚛️ FRONTEND - Aplicación React

## 📋 Índice

- [Descripción General](#descripción-general)
- [Tecnologías Utilizadas](#tecnologías-utilizadas)
- [Estructura de Archivos](#estructura-de-archivos)
- [Páginas Principales](#páginas-principales)
- [Servicios API](#servicios-api)
- [Componentes](#componentes)
- [Flujo de Datos](#flujo-de-datos)

---

## Descripción General

El **Frontend** es una aplicación web de una sola página (SPA) construida con React y TypeScript que proporciona una interfaz de usuario moderna e intuitiva para el sistema de inventario.

### Características Principales

- ✅ Interfaz moderna y responsiva
- ✅ Gestión completa de ingredientes, proveedores, órdenes y lotes
- ✅ Dashboard con estadísticas en tiempo real
- ✅ Modales para creación y edición
- ✅ Validación de formularios
- ✅ Alertas visuales para stock bajo y lotes próximos a vencer

---

## Tecnologías Utilizadas

| Tecnología       | Versión | Propósito        |
| ---------------- | ------- | ---------------- |
| **React**        | 18.x    | Framework UI     |
| **TypeScript**   | 5.x     | Type safety      |
| **Axios**        | 1.x     | Llamadas HTTP    |
| **React Router** | 6.x     | Navegación       |
| **React Icons**  | 5.x     | Iconografía      |
| **date-fns**     | 2.x     | Manejo de fechas |
| **CSS3**         | -       | Estilos          |

---

## Estructura de Archivos

```
frontend/
│
├── public/
│   └── index.html              # HTML principal
│
├── src/
│   ├── components/             # Componentes reutilizables
│   │   ├── Sidebar.tsx         # Menú lateral de navegación
│   │   └── ...
│   │
│   ├── pages/                  # Páginas principales
│   │   ├── Dashboard.tsx       # Panel principal con estadísticas
│   │   ├── Ingredientes.tsx    # Gestión de ingredientes
│   │   ├── Lotes.tsx           # Visualización de lotes
│   │   ├── Movimientos.tsx     # Historial de movimientos
│   │   ├── OrdenesCompra.tsx   # Gestión de órdenes de compra
│   │   ├── Proveedores.tsx     # Gestión de proveedores
│   │   ├── Categorias.tsx      # Gestión de categorías
│   │   └── CommonPages.css     # Estilos compartidos
│   │
│   ├── services/
│   │   └── api.ts              # Cliente HTTP y servicios
│   │
│   ├── types.ts                # Definiciones TypeScript
│   ├── App.tsx                 # Componente raíz
│   ├── index.tsx               # Punto de entrada
│   └── index.css               # Estilos globales
│
├── package.json
└── tsconfig.json
```

---

## Páginas Principales

### 📊 Dashboard.tsx

**Propósito**: Página principal que muestra estadísticas clave y resúmenes del inventario.

**Características**:

- 4 tarjetas de estadísticas:
  - Total de ingredientes
  - Ingredientes con stock bajo
  - Lotes próximos a vencer (30 días)
  - Órdenes pendientes de aprobación
- Tabla de ingredientes para reabastecer
- Tabla de lotes próximos a vencer
- Tabla de órdenes pendientes

**Servicios Utilizados**:

```typescript
// Carga datos del dashboard
const loadDashboardData = async () => {
  const stats = await dashboardService.getStats();
  const bajoStock = await ingredientesService.getParaReabastecer();
  const lotesVencer = await lotesService.getLotesVencenPronto(30);
  const ordenes = await ordenesCompraService.getPendientes();
};
```

**Validación de Fechas**:
El Dashboard implementa validación multi-capa para prevenir errores "Invalid time value":

1. Validación en servicios API
2. Validación antes de setState
3. Filtrado en renderizado

---

### 🥘 Ingredientes.tsx

**Propósito**: Gestión completa de ingredientes del inventario.

**Características**:

- Listado de todos los ingredientes con búsqueda
- Badges de estado de stock (Normal, Bajo Stock, Exceso)
- Modal para crear nuevos ingredientes
- Información detallada de cada ingrediente

**Modal de Creación**:

- Nombre y descripción
- Selección de categoría
- Unidad de medida (kg, L, unidad, etc.)
- Stock mínimo y máximo

**Estados de Stock**:

```typescript
const getStockStatus = (ing: Ingrediente) => {
  if (ing.cantidadEnStock < ing.stockMinimo) return "bajo";
  if (ing.cantidadEnStock > ing.stockMaximo) return "exceso";
  return "normal";
};
```

---

### 📦 Lotes.tsx

**Propósito**: Visualización y seguimiento de lotes de ingredientes.

**Características**:

- Lista completa de lotes con información enriquecida
- Muestra nombre del ingrediente (enriquecido desde API)
- Calcula días hasta vencimiento
- Badges visuales según proximidad a vencimiento:
  - 🔴 Rojo: < 7 días
  - 🟡 Amarillo: 7-30 días
  - 🟢 Verde: > 30 días

**Servicio Especial**:

```typescript
// Obtiene lotes con nombres de ingredientes
const data = await lotesService.getAllWithDetails();

// El servicio enriquece datos:
// - Busca ingredientes
// - Agrega nombre del ingrediente a cada lote
// - Calcula diasHastaVencimiento
```

---

### 📝 Movimientos.tsx

**Propósito**: Historial completo de movimientos de inventario.

**Características**:

- Lista de entradas y salidas
- Filtro por ingrediente o motivo
- Badges visuales para tipo de movimiento:
  - 🟢 Entrada (con icono hacia abajo)
  - 🔴 Salida (con icono hacia arriba)
- Muestra motivo, cantidad, fecha y usuario

**Estructura de Datos**:

```typescript
interface MovimientoInventario {
  id: string;
  ingredienteNombre: string;
  tipoMovimiento: "Entrada" | "Salida";
  cantidad: number;
  unidadMedida: string;
  motivo: string;
  fechaMovimiento: string;
  usuarioId: string;
}
```

---

### 🛒 OrdenesCompra.tsx

**Propósito**: Gestión del flujo completo de órdenes de compra.

**Características**:

- Listado de órdenes con información completa
- Modal para crear nuevas órdenes:
  - Selección de ingrediente
  - Selección de proveedor
  - Cantidad y precio unitario
  - Fecha esperada de entrega
- Botón de aprobar para órdenes pendientes
- Badges de estado: Pendiente, Aprobada, Recibida

**Flujo de Orden**:

```
1. Crear orden (estado: Pendiente)
   ↓
2. Aprobar orden (estado: Aprobada)
   ↓
3. Recibir orden (estado: Recibida)
   → Se crea lote automáticamente
   → Se actualiza stock
```

---

### 👥 Proveedores.tsx

**Propósito**: Gestión de proveedores.

**Características**:

- Listado de proveedores con información completa
- Modal para crear nuevos proveedores:
  - Nombre y NIT
  - Email y teléfono
  - Persona de contacto
  - Dirección completa (calle, ciudad, país, código postal)

---

### 📂 Categorias.tsx

**Propósito**: Gestión de categorías de ingredientes.

**Características**:

- Listado simple de categorías
- Modal para crear nuevas categorías
- Usado para organizar ingredientes

---

## Servicios API

### 📄 api.ts

**Propósito**: Centraliza todas las llamadas HTTP al backend.

**Estructura**:

```typescript
// Cliente HTTP base
const apiClient = axios.create({
  baseURL: "http://localhost:5261/api",
  headers: { "Content-Type": "application/json" },
});

// Servicios organizados por entidad
export const ingredientesService = {
  /* ... */
};
export const lotesService = {
  /* ... */
};
export const proveedoresService = {
  /* ... */
};
export const ordenesCompraService = {
  /* ... */
};
export const movimientosService = {
  /* ... */
};
export const categoriasService = {
  /* ... */
};
export const dashboardService = {
  /* ... */
};
```

**Servicios Clave**:

| Servicio               | Métodos Principales                     |
| ---------------------- | --------------------------------------- |
| `ingredientesService`  | getAll, create, getParaReabastecer      |
| `lotesService`         | getAllWithDetails, getLotesVencenPronto |
| `ordenesCompraService` | crear, aprobar, recibir, getPendientes  |
| `movimientosService`   | getAll, registrarConsumo                |
| `dashboardService`     | getStats (estadísticas agregadas)       |

**Validación de Fechas**:

```typescript
// Servicio getAllWithDetails con validación
getAllWithDetails: async (): Promise<any[]> => {
  const [lotesResponse, ingredientesResponse] = await Promise.all([
    apiClient.get("/lotes"),
    apiClient.get("/ingredientes"),
  ]);

  return lotes
    .filter((lote: any) => lote.fechaVencimiento) // Validación
    .map((lote: any) => ({
      ...lote,
      ingredienteNombre: ingredientesMap.get(lote.ingredienteId),
      diasHastaVencimiento: calcularDias(lote.fechaVencimiento),
    }));
};
```

---

## Componentes

### 🎨 Sidebar.tsx

**Propósito**: Menú lateral de navegación.

**Características**:

- Links a todas las páginas
- Iconos visuales para cada sección
- Indicador de página activa
- Responsive

**Links Principales**:

- Dashboard (🏠)
- Ingredientes (🥘)
- Lotes (📦)
- Movimientos (📝)
- Órdenes de Compra (🛒)
- Proveedores (👥)
- Categorías (📂)

---

## Flujo de Datos

### 1. Carga de Página

```
┌─────────────────────────────────────────┐
│ 1. Usuario accede a /ingredientes      │
└────────────┬────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────┐
│ 2. React Router renderiza              │
│    <Ingredientes /> component           │
└────────────┬────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────┐
│ 3. useEffect(() => {                   │
│      loadIngredientes();                │
│    }, []);                              │
└────────────┬────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────┐
│ 4. ingredientesService.getAll()        │
│    → GET http://localhost:5261/api/...  │
└────────────┬────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────┐
│ 5. Backend API responde con JSON       │
└────────────┬────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────┐
│ 6. setIngredientes(data)               │
│    → Estado actualizado                 │
└────────────┬────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────┐
│ 7. React re-renderiza con datos        │
└─────────────────────────────────────────┘
```

---

### 2. Crear Ingrediente

```
┌─────────────────────────────────────────┐
│ 1. Usuario llena formulario y          │
│    hace clic en "Crear"                 │
└────────────┬────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────┐
│ 2. handleCreate()                       │
│    Valida datos del formulario          │
└────────────┬────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────┐
│ 3. ingredientesService.create(data)    │
│    → POST http://localhost:5261/api/... │
└────────────┬────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────┐
│ 4. Backend procesa                      │
│    CrearIngredienteCommand              │
└────────────┬────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────┐
│ 5. Respuesta 201 Created               │
│    con datos del ingrediente            │
└────────────┬────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────┐
│ 6. Cerrar modal                        │
│    loadIngredientes() - Recargar lista │
└─────────────────────────────────────────┘
```

---

## Patrones de Diseño Utilizados

### ✅ Hooks de React

**useState** - Gestión de estado local:

```typescript
const [ingredientes, setIngredientes] = useState<Ingrediente[]>([]);
const [loading, setLoading] = useState(true);
```

**useEffect** - Efectos secundarios (cargar datos):

```typescript
useEffect(() => {
  loadIngredientes();
}, []);
```

---

### ✅ Async/Await

Todas las llamadas API usan async/await:

```typescript
const loadIngredientes = async () => {
  try {
    setLoading(true);
    const data = await ingredientesService.getAll();
    setIngredientes(data);
  } catch (error) {
    console.error("Error:", error);
  } finally {
    setLoading(false);
  }
};
```

---

### ✅ Validación Multi-Capa

Para prevenir errores de fechas inválidas:

```typescript
// 1. Validación en servicio
.filter((lote: any) => lote.fechaVencimiento)

// 2. Validación en carga de datos
const lotesValidos = (data || []).filter((lote: any) => {
  if (!lote.fechaVencimiento) return false;
  return !isNaN(new Date(lote.fechaVencimiento).getTime());
});

// 3. Validación en renderizado
{lotes.filter(l => l.fechaVencimiento).map(lote => ...)}
```

---

### ✅ Modales Reutilizables

Patrón común para modales:

```typescript
const [showModal, setShowModal] = useState(false);

// Abrir modal
<button onClick={() => setShowModal(true)}>Nuevo</button>;

// Modal
{
  showModal && (
    <div className="modal-overlay">
      <div className="modal-content">{/* Formulario */}</div>
    </div>
  );
}
```

---

## Estilos CSS

### CommonPages.css

**Propósito**: Estilos compartidos para todas las páginas.

**Clases Principales**:

- `.page` - Contenedor principal de página
- `.page-header` - Encabezado con título y botones
- `.card` - Tarjeta contenedora
- `.table-container` - Contenedor de tabla con scroll
- `.badge` - Badges de estado (success, warning, danger)
- `.modal-overlay` - Fondo oscuro del modal
- `.modal-content` - Contenido del modal
- `.form-group` - Grupo de formulario

**Badges de Estado**:

```css
.badge {
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 0.85rem;
}

.badge-success {
  background: #d4edda;
  color: #155724;
}
.badge-warning {
  background: #fff3cd;
  color: #856404;
}
.badge-danger {
  background: #f8d7da;
  color: #721c24;
}
```

---

## Manejo de Errores

### Try-Catch en Servicios

```typescript
try {
  await ingredientesService.create(data);
  alert("✅ Ingrediente creado exitosamente");
  setShowModal(false);
  loadIngredientes();
} catch (error: any) {
  alert("❌ Error: " + (error.response?.data?.message || error.message));
}
```

### Estados de Carga

```typescript
if (loading) {
  return (
    <div className="loading">
      <div className="spinner"></div>
    </div>
  );
}
```

---

## Resumen

El **Frontend React** proporciona:

1. ✅ **Interfaz moderna** con React 18 y TypeScript
2. ✅ **Gestión completa** de inventario (CRUD)
3. ✅ **Dashboard** con estadísticas en tiempo real
4. ✅ **Validación robusta** de fechas y datos
5. ✅ **Modales** para creación/edición
6. ✅ **Alertas visuales** para stock y vencimientos
7. ✅ **Servicios centralizados** en api.ts
8. ✅ **Manejo de errores** apropiado

**Arquitectura**: SPA (Single Page Application) que consume API REST del backend .NET.
