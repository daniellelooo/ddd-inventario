import React, { useEffect, useState } from "react";
import { FiPlus, FiX } from "react-icons/fi";
import {
  ordenesCompraService,
  ingredientesService,
  proveedoresService,
} from "../services/api";
import { OrdenDeCompra, Ingrediente, Proveedor } from "../types";
import "./CommonPages.css";
import { format } from "date-fns";

const OrdenesCompra: React.FC = () => {
  const [ordenes, setOrdenes] = useState<OrdenDeCompra[]>([]);
  const [ingredientes, setIngredientes] = useState<Ingrediente[]>([]);
  const [proveedores, setProveedores] = useState<Proveedor[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);

  const [newOrden, setNewOrden] = useState({
    ingredienteId: "",
    proveedorId: "",
    cantidad: 0,
    precioUnitario: 0,
    fechaEsperada: "",
  });

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [ordenesData, ingredientesData, proveedoresData] =
        await Promise.all([
          ordenesCompraService.getPendientes(),
          ingredientesService.getAll(),
          proveedoresService.getAll(),
        ]);
      setOrdenes(ordenesData);
      setIngredientes(ingredientesData);
      setProveedores(proveedoresData);
    } catch (error) {
      console.error("Error:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleAprobar = async (ordenId: string) => {
    try {
      await ordenesCompraService.aprobar(ordenId, "sistema");
      await loadData();
    } catch (error) {
      console.error("Error:", error);
    }
  };

  const handleCreate = async () => {
    try {
      await ordenesCompraService.crear({
        proveedorId: newOrden.proveedorId,
        detalles: [
          {
            ingredienteId: newOrden.ingredienteId,
            cantidad: newOrden.cantidad,
            precioUnitario: newOrden.precioUnitario,
            moneda: "COP",
          },
        ],
        fechaEsperada: newOrden.fechaEsperada,
      });
      setShowModal(false);
      setNewOrden({
        ingredienteId: "",
        proveedorId: "",
        cantidad: 0,
        precioUnitario: 0,
        fechaEsperada: "",
      });
      await loadData();
    } catch (error) {
      console.error("Error:", error);
    }
  };

  if (loading) {
    return (
      <div className="loading">
        <div className="spinner"></div>
      </div>
    );
  }

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Órdenes de Compra</h1>
          <p>Gestión de órdenes de compra pendientes</p>
        </div>
        <button className="btn btn-primary" onClick={() => setShowModal(true)}>
          <FiPlus /> Nueva Orden
        </button>
      </div>

      <div className="card">
        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>Número</th>
                <th>Proveedor</th>
                <th>Ingrediente</th>
                <th>Cantidad</th>
                <th>Precio Unit.</th>
                <th>Total</th>
                <th>Estado</th>
                <th>Fecha Esperada</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {ordenes.length === 0 ? (
                <tr>
                  <td colSpan={9} style={{ textAlign: "center" }}>
                    No hay órdenes de compra pendientes
                  </td>
                </tr>
              ) : (
                ordenes.map((orden) => (
                  <tr key={orden.id}>
                    <td>
                      <strong>{orden.numero || "N/A"}</strong>
                    </td>
                    <td>{orden.proveedorNombre || "N/A"}</td>
                    <td>{orden.ingredienteNombre || "N/A"}</td>
                    <td>{orden.cantidad || 0}</td>
                    <td>${orden.precioUnitario?.toLocaleString() || "0"}</td>
                    <td>
                      <strong>
                        ${orden.total?.toLocaleString() || "0"}{" "}
                        {orden.moneda || "COP"}
                      </strong>
                    </td>
                    <td>
                      <span
                        className={`badge ${
                          orden.estado === "Pendiente"
                            ? "badge-warning"
                            : orden.estado === "Aprobada"
                            ? "badge-info"
                            : orden.estado === "Recibida"
                            ? "badge-success"
                            : "badge-secondary"
                        }`}
                      >
                        {orden.estado || "Desconocido"}
                      </span>
                    </td>
                    <td>
                      {orden.fechaEsperada
                        ? format(new Date(orden.fechaEsperada), "dd/MM/yyyy")
                        : "N/A"}
                    </td>
                    <td>
                      {orden.estado === "Pendiente" && (
                        <button
                          className="btn btn-success btn-sm"
                          onClick={() => handleAprobar(orden.id)}
                        >
                          Aprobar
                        </button>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>Nueva Orden de Compra</h2>
              <button className="close-btn" onClick={() => setShowModal(false)}>
                <FiX />
              </button>
            </div>
            <div className="modal-body">
              <div className="form-group">
                <label>Ingrediente *</label>
                <select
                  value={newOrden.ingredienteId}
                  onChange={(e) =>
                    setNewOrden({ ...newOrden, ingredienteId: e.target.value })
                  }
                  required
                >
                  <option value="">Seleccione...</option>
                  {ingredientes.map((ing) => (
                    <option key={ing.id} value={ing.id}>
                      {ing.nombre}
                    </option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label>Proveedor *</label>
                <select
                  value={newOrden.proveedorId}
                  onChange={(e) =>
                    setNewOrden({ ...newOrden, proveedorId: e.target.value })
                  }
                  required
                >
                  <option value="">Seleccione...</option>
                  {proveedores.map((prov) => (
                    <option key={prov.id} value={prov.id}>
                      {prov.nombre}
                    </option>
                  ))}
                </select>
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>Cantidad *</label>
                  <input
                    type="number"
                    value={newOrden.cantidad}
                    onChange={(e) =>
                      setNewOrden({
                        ...newOrden,
                        cantidad: Number(e.target.value),
                      })
                    }
                    required
                  />
                </div>

                <div className="form-group">
                  <label>Precio Unitario *</label>
                  <input
                    type="number"
                    value={newOrden.precioUnitario}
                    onChange={(e) =>
                      setNewOrden({
                        ...newOrden,
                        precioUnitario: Number(e.target.value),
                      })
                    }
                    required
                  />
                </div>
              </div>

              <div className="form-group">
                <label>Fecha Esperada *</label>
                <input
                  type="date"
                  value={newOrden.fechaEsperada}
                  onChange={(e) =>
                    setNewOrden({ ...newOrden, fechaEsperada: e.target.value })
                  }
                  required
                />
              </div>
            </div>
            <div className="modal-footer">
              <button
                className="btn btn-secondary"
                onClick={() => setShowModal(false)}
              >
                Cancelar
              </button>
              <button className="btn btn-primary" onClick={handleCreate}>
                Crear Orden
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default OrdenesCompra;
