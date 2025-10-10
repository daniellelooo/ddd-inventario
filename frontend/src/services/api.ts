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
    return response.data.data || response.data;
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
    const raw = response.data?.data ?? response.data ?? [];
    // Normalizar propiedades del backend (DiasHastaVencimiento vs diasParaVencer, nombres, etc.)
    return (Array.isArray(raw) ? raw : []).map((l: any) => ({
      id: String(l.id ?? l.Id ?? ''),
      codigo: String(l.codigo ?? l.Codigo ?? ''),
      ingredienteNombre: String(l.ingredienteNombre ?? l.IngredienteNombre ?? ''),
      cantidadDisponible: Number(l.cantidadDisponible ?? l.CantidadDisponible ?? l.cantidad ?? 0),
      fechaVencimiento: String(l.fechaVencimiento ?? l.FechaVencimiento ?? ''),
      diasParaVencer: Number(l.diasParaVencer ?? l.DiasHastaVencimiento ?? 0),
      proveedorNombre: String(l.proveedorNombre ?? l.ProveedorNombre ?? ''),
      precioUnitario: Number(l.precioUnitario ?? l.PrecioUnitario ?? 0),
      moneda: String(l.moneda ?? l.Moneda ?? ''),
    }));
  },

  getByIngrediente: async (ingredienteId: string): Promise<Lote[]> => {
    const response = await apiClient.get(`/lotes/ingrediente/${ingredienteId}`);
    return response.data;
  },

  create: async (lote: Partial<Lote>): Promise<Lote> => {
    // No hay endpoint directo para crear lotes; se crea vía orden de compra -> aprobar -> recibir
    // Requiere: ingredienteId, proveedorId, cantidadInicial, fechaVencimiento, codigo
    if (!lote.ingredienteId || !lote.proveedorId || !lote.cantidadInicial || !lote.fechaVencimiento) {
      throw new Error('Faltan datos para crear el lote (ingrediente, proveedor, cantidad y fecha de vencimiento son requeridos)');
    }

    const codigoLote = lote.codigo || `Lote-${Date.now()}`;
    const fechaEsperada = new Date().toISOString().slice(0, 10);

    // 1) Crear orden de compra con un único detalle
    const command: CrearOrdenDeCompraCommand = {
      proveedorId: lote.proveedorId,
      fechaEsperada,
      detalles: [
        {
          ingredienteId: lote.ingredienteId,
          cantidad: Number(lote.cantidadInicial),
          precioUnitario: 0,
          moneda: 'BOB',
        },
      ],
    };

    const ordenId = await ordenesCompraService.crear(command);

    // 2) Aprobar la orden
    await ordenesCompraService.aprobar(ordenId, 'ui');

    // 3) Recibir la orden para crear el lote
    const recibirResp = await ordenesCompraService.recibir(ordenId, {
      lotes: [
        {
          ingredienteId: lote.ingredienteId,
          codigoLote,
          cantidad: Number(lote.cantidadInicial),
          fechaVencimiento: lote.fechaVencimiento,
        }
      ]
    });

    // 4) Consultar lotes y devolver el recién creado (por código), con reintentos cortos
    const loteId = recibirResp?.data?.loteId;
    const normalizeDate = (d: string) => (d ? new Date(d).toISOString().slice(0, 10) : d);
    const targetDate = normalizeDate(String(lote.fechaVencimiento));

    const retries = 3;
    for (let i = 0; i < retries; i++) {
      const all = await lotesService.getAll();
      let creado = all.find((l) => l.codigo === codigoLote);
      if (!creado && loteId) {
        creado = all.find((l) => l.id === loteId);
      }
      if (!creado) {
        creado = all.find(
          (l) => l.ingredienteId === lote.ingredienteId && normalizeDate(String(l.fechaVencimiento)) === targetDate
        );
      }
      if (creado) return creado;
      // pequeña espera entre intentos (no bloqueante real; aquí solo reintenta sin delay)
    }
    throw new Error('Lote creado pero no se pudo recuperar la información');
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/lotes/${id}`);
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

  crear: async (command: CrearOrdenDeCompraCommand): Promise<string> => {
    const response = await apiClient.post('/ordenescompra', command);
    // Esperamos { success, message, data: { ordenId } } o variantes
    const payload = response.data as any;
    const ordenId = payload?.data?.ordenId || payload?.ordenId || payload?.id;
    if (!ordenId) {
      throw new Error('No se pudo obtener el ID de la orden de compra creada');
    }
    return String(ordenId);
  },

  aprobar: async (ordenId: string, usuarioId: string): Promise<void> => {
    await apiClient.post(`/ordenescompra/${ordenId}/aprobar`, { usuarioId });
  },

  recibir: async (ordenId: string, data: any): Promise<any> => {
    const response = await apiClient.post(`/ordenescompra/${ordenId}/recibir`, data);
    return response.data; // { success, message, data: { loteId } }
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
    const results = await Promise.allSettled([
      ingredientesService.getAll(),
      ingredientesService.getParaReabastecer(),
      lotesService.getProximosVencer(7),
      ordenesCompraService.getPendientes(),
    ]);

    const safe = (idx: number) => {
      const r = results[idx];
      return r.status === 'fulfilled' ? r.value : [];
    };

    const ingredientes = safe(0) as Ingrediente[];
    const bajoStock = safe(1) as IngredienteParaReabastecer[];
    const lotesVencer = safe(2) as LoteProximoVencer[];
    const ordenesPendientes = safe(3) as OrdenDeCompra[];

    return {
      totalIngredientes: ingredientes.length,
      ingredientesBajoStock: bajoStock.length,
      lotesProximosVencer: lotesVencer.length,
      ordenesPendientes: ordenesPendientes.length,
      valorInventario: 0, // Calcular si es necesario
    };
  },
};
