import React, { useEffect, useState } from "react";
import { FiPlus } from "react-icons/fi";
import { ordenesCompraService } from "../services/api";
import { OrdenDeCompra } from "../types";
import "./CommonPages.css";
import { format } from "date-fns";

const OrdenesCompra: React.FC = () => {
  const [ordenes, setOrdenes] = useState<OrdenDeCompra[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadOrdenes();
  }, []);

  const loadOrdenes = async () => {
    try {
      const data = await ordenesCompraService.getPendientes();
      setOrdenes(data);
    } catch (error) {
      console.error("Error:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleAprobar = async (ordenId: string) => {
    try {
      await ordenesCompraService.aprobar(ordenId, "sistema");
      await loadOrdenes();
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
        <button className="btn btn-primary">
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
              {ordenes.map((orden) => (
                <tr key={orden.id}>
                  <td>
                    <strong>{orden.numero}</strong>
                  </td>
                  <td>{orden.proveedorNombre}</td>
                  <td>{orden.ingredienteNombre}</td>
                  <td>{orden.cantidad}</td>
                  <td>${orden.precioUnitario.toLocaleString()}</td>
                  <td>
                    <strong>
                      ${orden.total.toLocaleString()} {orden.moneda}
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
                      {orden.estado}
                    </span>
                  </td>
                  <td>{format(new Date(orden.fechaEsperada), "dd/MM/yyyy")}</td>
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
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default OrdenesCompra;
