// ===== TIPOS DE DOMINIO =====

export interface Ingrediente {
  id: string;
  nombre: string;
  descripcion: string;
  unidadMedida: UnidadMedida;
  cantidadEnStock: number;
  stockMinimo: number;
  stockMaximo: number;
  categoriaId: string;
  categoriaNombre?: string;
  activo: boolean;
  fechaCreacion: string;
}

export interface UnidadMedida {
  nombre: string;
  simbolo: string;
}

export interface Lote {
  id: string;
  codigo: string;
  ingredienteId: string;
  ingredienteNombre?: string;
  proveedorId: string;
  proveedorNombre?: string;
  cantidadInicial: number;
  cantidadDisponible: number;
  fechaVencimiento: string;
  fechaRecepcion: string;
  precioUnitario: number;
  moneda: string;
  vencido: boolean;
}

export interface Proveedor {
  id: string;
  nombre: string;
  nit: string;
  telefono: string;
  email: string;
  direccion: DireccionProveedor;
  personaContacto: string;
  activo: boolean;
  fechaRegistro: string;
}

export interface DireccionProveedor {
  calle: string;
  ciudad: string;
  pais: string;
  codigoPostal?: string;
}

export interface OrdenDeCompra {
  id: string;
  numero: string;
  ingredienteId: string;
  ingredienteNombre?: string;
  proveedorId: string;
  proveedorNombre?: string;
  cantidad: number;
  precioUnitario: number;
  moneda: string;
  estado: EstadoOrden;
  fechaCreacion: string;
  fechaEsperada: string;
  fechaRecepcion?: string;
  observaciones?: string;
  total: number;
}

export enum EstadoOrden {
  Pendiente = 'Pendiente',
  Aprobada = 'Aprobada',
  Recibida = 'Recibida',
  Cancelada = 'Cancelada'
}

export interface MovimientoInventario {
  id: string;
  ingredienteId: string;
  ingredienteNombre?: string;
  loteId?: string;
  loteCodigo?: string;
  tipoMovimiento: TipoMovimiento;
  cantidad: number;
  unidadMedida: UnidadMedida;
  fechaMovimiento: string;
  motivo: string;
  documentoReferencia?: string;
  usuarioId?: string;
  observaciones?: string;
}

export enum TipoMovimiento {
  Entrada = 'Entrada',
  Salida = 'Salida',
  Ajuste = 'Ajuste'
}

export interface Categoria {
  id: string;
  nombre: string;
  descripcion: string;
  activa: boolean;
  fechaCreacion: string;
}

// ===== DTOs PARA COMANDOS =====

export interface CrearOrdenDeCompraCommand {
  proveedorId: string;
  detalles: DetalleOrdenCompra[];
  fechaEsperada: string;
  observaciones?: string;
}

export interface DetalleOrdenCompra {
  ingredienteId: string;
  cantidad: number;
  precioUnitario: number;
  moneda: string;
}

export interface AprobarOrdenDeCompraCommand {
  ordenId: string;
  usuarioId: string;
}

export interface RecibirOrdenDeCompraCommand {
  ordenId: string;
  loteData: RecibirLoteData;
}

export interface RecibirLoteData {
  codigoLote: string;
  fechaVencimiento: string;
  observaciones?: string;
}

export interface RegistrarConsumoCommand {
  ingredienteId: string;
  cantidad: number;
  motivo: string;
  usuarioId?: string;
  observaciones?: string;
}

// ===== RESPUESTAS DE API =====

export interface ApiResponse<T> {
  data?: T;
  success: boolean;
  message?: string;
  errors?: string[];
}

export interface PaginatedResponse<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

// ===== FILTROS Y QUERIES =====

export interface IngredientesFilter {
  categoriaId?: string;
  activo?: boolean;
  bajoStock?: boolean;
  search?: string;
}

export interface OrdenesFilter {
  estado?: EstadoOrden;
  proveedorId?: string;
  fechaDesde?: string;
  fechaHasta?: string;
}

export interface MovimientosFilter {
  ingredienteId?: string;
  tipoMovimiento?: TipoMovimiento;
  fechaDesde?: string;
  fechaHasta?: string;
}

// ===== ESTADÍSTICAS Y DASHBOARD =====

export interface DashboardStats {
  totalIngredientes: number;
  ingredientesBajoStock: number;
  lotesProximosVencer: number;
  ordenesPendientes: number;
  valorInventario: number;
}

export interface IngredienteParaReabastecer {
  id: string;
  nombre: string;
  cantidadActual: number;
  stockMinimo: number;
  stockMaximo: number;
  cantidadSugerida: number;
  unidadMedida: UnidadMedida;
  categoriaNombre: string;
}

export interface LoteProximoVencer {
  id: string;
  codigo: string;
  ingredienteNombre: string;
  cantidadDisponible: number;
  fechaVencimiento: string;
  diasParaVencer: number;
  proveedorNombre: string;
  precioUnitario: number;
  moneda: string;
}

// ===== UI STATE =====

export interface LoadingState {
  isLoading: boolean;
  message?: string;
}

export interface ErrorState {
  hasError: boolean;
  message?: string;
  details?: string[];
}

export interface NotificationState {
  show: boolean;
  type: 'success' | 'error' | 'warning' | 'info';
  message: string;
}
