import apiClient from './apiClient';
import {
  Ingrediente,
  Lote,
  Proveedor,
  OrdenDeCompra,
  MovimientoInventario,
  Categoria,
  CrearOrdenDeCompraCommand,
  RegistrarConsumoCommand,
  IngredienteParaReabastecer,
  LoteProximoVencer,
} from '../types';

// ===== INGREDIENTES =====
export const ingredientesService = {
  getAll: async (): Promise<Ingrediente[]> => {
    const response = await apiClient.get('/ingredientes');
    return response.data;
  },

  getById: async (id: string): Promise<Ingrediente> => {
    const response = await apiClient.get(`/ingredientes/${id}`);
    return response.data;
  },

  getParaReabastecer: async (): Promise<IngredienteParaReabastecer[]> => {
    const response = await apiClient.get('/ingredientes/reabastecer');
    return response.data;
  },

  create: async (ingrediente: Partial<Ingrediente>): Promise<Ingrediente> => {
    const response = await apiClient.post('/ingredientes', ingrediente);
    return response.data;
  },

  update: async (id: string, ingrediente: Partial<Ingrediente>): Promise<Ingrediente> => {
    const response = await apiClient.put(`/ingredientes/${id}`, ingrediente);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/ingredientes/${id}`);
  },
};

// ===== LOTES =====
export const lotesService = {
  getAll: async (): Promise<Lote[]> => {
    const response = await apiClient.get('/lotes');
    return response.data;
  },

  getById: async (id: string): Promise<Lote> => {
    const response = await apiClient.get(`/lotes/${id}`);
    return response.data;
  },

  getProximosVencer: async (dias: number = 30): Promise<LoteProximoVencer[]> => {
    const response = await apiClient.get(`/inventario/lotes/proximos-vencer?diasAnticipacion=${dias}`);
    return response.data.data || response.data;
  },

  getByIngrediente: async (ingredienteId: string): Promise<Lote[]> => {
    const response = await apiClient.get(`/lotes/ingrediente/${ingredienteId}`);
    return response.data;
  },
};

// ===== PROVEEDORES =====
export const proveedoresService = {
  getAll: async (): Promise<Proveedor[]> => {
    const response = await apiClient.get('/proveedores');
    return response.data;
  },

  getById: async (id: string): Promise<Proveedor> => {
    const response = await apiClient.get(`/proveedores/${id}`);
    return response.data;
  },

  getActivos: async (): Promise<Proveedor[]> => {
    const response = await apiClient.get('/proveedores/activos');
    return response.data;
  },

  create: async (proveedor: Partial<Proveedor>): Promise<Proveedor> => {
    const response = await apiClient.post('/proveedores', proveedor);
    return response.data;
  },

  update: async (id: string, proveedor: Partial<Proveedor>): Promise<Proveedor> => {
    const response = await apiClient.put(`/proveedores/${id}`, proveedor);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/proveedores/${id}`);
  },
};

// ===== ÓRDENES DE COMPRA =====
export const ordenesCompraService = {
  getAll: async (): Promise<OrdenDeCompra[]> => {
    const response = await apiClient.get('/ordenescompra');
    return response.data.data || response.data;
  },

  getById: async (id: string): Promise<OrdenDeCompra> => {
    const response = await apiClient.get(`/ordenescompra/${id}`);
    return response.data.data || response.data;
  },

  getPendientes: async (): Promise<OrdenDeCompra[]> => {
    const response = await apiClient.get('/ordenescompra/pendientes');
    return response.data.data || response.data;
  },

  crear: async (command: CrearOrdenDeCompraCommand): Promise<OrdenDeCompra> => {
    const response = await apiClient.post('/ordenescompra', command);
    return response.data;
  },

  aprobar: async (ordenId: string, usuarioId: string): Promise<void> => {
    await apiClient.post(`/ordenescompra/${ordenId}/aprobar`, { usuarioId });
  },

  recibir: async (ordenId: string, data: any): Promise<void> => {
    await apiClient.post(`/ordenescompra/${ordenId}/recibir`, data);
  },

  cancelar: async (ordenId: string): Promise<void> => {
    await apiClient.post(`/ordenescompra/${ordenId}/cancelar`);
  },
};

// ===== MOVIMIENTOS DE INVENTARIO =====
export const movimientosService = {
  getAll: async (): Promise<MovimientoInventario[]> => {
    const response = await apiClient.get('/movimientos');
    return response.data;
  },

  getById: async (id: string): Promise<MovimientoInventario> => {
    const response = await apiClient.get(`/movimientos/${id}`);
    return response.data;
  },

  getHistorial: async (ingredienteId: string): Promise<MovimientoInventario[]> => {
    const response = await apiClient.get(`/inventario/movimientos/${ingredienteId}`);
    return response.data.data || response.data;
  },

  registrarConsumo: async (command: RegistrarConsumoCommand): Promise<void> => {
    await apiClient.post('/inventario/consumo', command);
  },
};

// ===== CATEGORÍAS =====
export const categoriasService = {
  getAll: async (): Promise<Categoria[]> => {
    const response = await apiClient.get('/categorias');
    return response.data;
  },

  getById: async (id: string): Promise<Categoria> => {
    const response = await apiClient.get(`/categorias/${id}`);
    return response.data;
  },

  getActivas: async (): Promise<Categoria[]> => {
    const response = await apiClient.get('/categorias/activas');
    return response.data;
  },

  create: async (categoria: Partial<Categoria>): Promise<Categoria> => {
    const response = await apiClient.post('/categorias', categoria);
    return response.data;
  },

  update: async (id: string, categoria: Partial<Categoria>): Promise<Categoria> => {
    const response = await apiClient.put(`/categorias/${id}`, categoria);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/categorias/${id}`);
  },
};

// ===== DASHBOARD =====
export const dashboardService = {
  getStats: async () => {
    const [ingredientes, bajoStock, lotesVencer, ordenesPendientes] = await Promise.all([
      ingredientesService.getAll(),
      ingredientesService.getParaReabastecer(),
      lotesService.getProximosVencer(7),
      ordenesCompraService.getPendientes(),
    ]);

    return {
      totalIngredientes: ingredientes.length,
      ingredientesBajoStock: bajoStock.length,
      lotesProximosVencer: lotesVencer.length,
      ordenesPendientes: ordenesPendientes.length,
      valorInventario: 0, // Calcular si es necesario
    };
  },
};
