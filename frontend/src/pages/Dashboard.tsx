import React, { useEffect, useState } from "react";
import {
  FiPackage,
  FiAlertTriangle,
  FiClock,
  FiShoppingCart,
  FiTrendingUp,
  FiTrendingDown,
} from "react-icons/fi";
import {
  dashboardService,
  ingredientesService,
  lotesService,
  ordenesCompraService,
} from "../services/api";
import {
  IngredienteParaReabastecer,
  LoteProximoVencer,
  OrdenDeCompra,
} from "../types";
import "./Dashboard.css";
import { format } from "date-fns";
import { es } from "date-fns/locale";

const Dashboard: React.FC = () => {
  const [stats, setStats] = useState({
    totalIngredientes: 0,
    ingredientesBajoStock: 0,
    lotesProximosVencer: 0,
    ordenesPendientes: 0,
  });
  const [ingredientesReabastecer, setIngredientesReabastecer] = useState<
    IngredienteParaReabastecer[]
  >([]);
  const [lotesVencer, setLotesVencer] = useState<LoteProximoVencer[]>([]);
  const [ordenesPendientes, setOrdenesPendientes] = useState<OrdenDeCompra[]>(
    []
  );
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      setLoading(true);
      const [statsData, reabastecer, vencer, pendientes] = await Promise.all([
        dashboardService.getStats(),
        ingredientesService.getParaReabastecer(),
        lotesService.getProximosVencer(7),
        ordenesCompraService.getPendientes(),
      ]);

      setStats(statsData);
      setIngredientesReabastecer(reabastecer.slice(0, 5));
      setLotesVencer(vencer.slice(0, 5));
      setOrdenesPendientes(pendientes.slice(0, 5));
    } catch (error) {
      console.error("Error loading dashboard:", error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="loading">
        <div className="spinner"></div>
        <p>Cargando dashboard...</p>
      </div>
    );
  }

  return (
    <div className="dashboard">
      <div className="dashboard-header">
        <h1>Dashboard de Inventario</h1>
        <p>Resumen general del estado del inventario</p>
      </div>

      {/* Tarjetas de estadísticas */}
      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-icon stat-icon-primary">
            <FiPackage />
          </div>
          <div className="stat-info">
            <h3>Total Ingredientes</h3>
            <p className="stat-value">{stats.totalIngredientes}</p>
          </div>
          <div className="stat-trend stat-trend-up">
            <FiTrendingUp /> +5%
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon stat-icon-warning">
            <FiAlertTriangle />
          </div>
          <div className="stat-info">
            <h3>Bajo Stock</h3>
            <p className="stat-value">{stats.ingredientesBajoStock}</p>
          </div>
          <div className="stat-trend stat-trend-down">
            <FiTrendingDown /> -2%
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon stat-icon-danger">
            <FiClock />
          </div>
          <div className="stat-info">
            <h3>Próximos a Vencer</h3>
            <p className="stat-value">{stats.lotesProximosVencer}</p>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon stat-icon-info">
            <FiShoppingCart />
          </div>
          <div className="stat-info">
            <h3>Órdenes Pendientes</h3>
            <p className="stat-value">{stats.ordenesPendientes}</p>
          </div>
        </div>
      </div>

      <div className="dashboard-content">
        {/* Ingredientes para reabastecer */}
        <div className="dashboard-section">
          <div className="section-header">
            <h2>Ingredientes para Reabastecer</h2>
            <button className="btn btn-outline">Ver Todos</button>
          </div>
          <div className="card">
            {ingredientesReabastecer.length === 0 ? (
              <p className="empty-state">
                No hay ingredientes que necesiten reabastecimiento
              </p>
            ) : (
              <div className="table-container">
                <table>
                  <thead>
                    <tr>
                      <th>Ingrediente</th>
                      <th>Categoría</th>
                      <th>Stock Actual</th>
                      <th>Stock Mínimo</th>
                      <th>Sugerido</th>
                      <th>Acción</th>
                    </tr>
                  </thead>
                  <tbody>
                    {ingredientesReabastecer.map((ing) => (
                      <tr key={ing.id}>
                        <td>
                          <strong>{ing.nombre}</strong>
                        </td>
                        <td>{ing.categoriaNombre}</td>
                        <td>
                          <span className="badge badge-warning">
                            {ing.cantidadActual} {ing.unidadMedida.simbolo}
                          </span>
                        </td>
                        <td>
                          {ing.stockMinimo} {ing.unidadMedida.simbolo}
                        </td>
                        <td>
                          <strong>
                            {ing.cantidadSugerida} {ing.unidadMedida.simbolo}
                          </strong>
                        </td>
                        <td>
                          <button className="btn btn-primary btn-sm">
                            Crear Orden
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>

        {/* Lotes próximos a vencer */}
        <div className="dashboard-section">
          <div className="section-header">
            <h2>Lotes Próximos a Vencer (7 días)</h2>
            <button className="btn btn-outline">Ver Todos</button>
          </div>
          <div className="card">
            {lotesVencer.length === 0 ? (
              <p className="empty-state">No hay lotes próximos a vencer</p>
            ) : (
              <div className="table-container">
                <table>
                  <thead>
                    <tr>
                      <th>Código</th>
                      <th>Ingrediente</th>
                      <th>Cantidad</th>
                      <th>Fecha Vencimiento</th>
                      <th>Días Restantes</th>
                      <th>Estado</th>
                    </tr>
                  </thead>
                  <tbody>
                    {lotesVencer.map((lote) => (
                      <tr key={lote.id}>
                        <td>
                          <strong>{lote.codigo}</strong>
                        </td>
                        <td>{lote.ingredienteNombre}</td>
                        <td>{lote.cantidadDisponible}</td>
                        <td>
                          {format(
                            new Date(lote.fechaVencimiento),
                            "dd/MM/yyyy",
                            { locale: es }
                          )}
                        </td>
                        <td>
                          <span
                            className={`badge ${
                              lote.diasParaVencer <= 3
                                ? "badge-danger"
                                : "badge-warning"
                            }`}
                          >
                            {lote.diasParaVencer} días
                          </span>
                        </td>
                        <td>
                          <span className="badge badge-warning">Urgente</span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>

        {/* Órdenes pendientes */}
        <div className="dashboard-section">
          <div className="section-header">
            <h2>Órdenes de Compra Pendientes</h2>
            <button className="btn btn-outline">Ver Todas</button>
          </div>
          <div className="card">
            {ordenesPendientes.length === 0 ? (
              <p className="empty-state">No hay órdenes pendientes</p>
            ) : (
              <div className="table-container">
                <table>
                  <thead>
                    <tr>
                      <th>Número</th>
                      <th>Proveedor</th>
                      <th>Ingrediente</th>
                      <th>Cantidad</th>
                      <th>Total</th>
                      <th>Estado</th>
                      <th>Fecha Esperada</th>
                    </tr>
                  </thead>
                  <tbody>
                    {ordenesPendientes.map((orden) => (
                      <tr key={orden.id}>
                        <td>
                          <strong>{orden.numero}</strong>
                        </td>
                        <td>{orden.proveedorNombre}</td>
                        <td>{orden.ingredienteNombre}</td>
                        <td>{orden.cantidad}</td>
                        <td>
                          <strong>
                            ${orden.total.toLocaleString()} {orden.moneda}
                          </strong>
                        </td>
                        <td>
                          <span className="badge badge-warning">
                            {orden.estado}
                          </span>
                        </td>
                        <td>
                          {format(new Date(orden.fechaEsperada), "dd/MM/yyyy", {
                            locale: es,
                          })}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
