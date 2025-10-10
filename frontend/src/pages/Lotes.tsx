import React, { useEffect, useState } from "react";
import { FiSearch } from "react-icons/fi";
import { lotesService } from "../services/api";
import { LoteProximoVencer } from "../types";
import "./CommonPages.css";
import { format } from "date-fns";

const Lotes: React.FC = () => {
  const [lotes, setLotes] = useState<LoteProximoVencer[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadLotes();
  }, []);

  const loadLotes = async () => {
    try {
      const data = await lotesService.getProximosVencer(365); // Todos los lotes
      setLotes(data);
    } catch (error) {
      console.error("Error:", error);
    } finally {
      setLoading(false);
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
          <h1>Lotes</h1>
          <p>Control de lotes de ingredientes</p>
        </div>
      </div>

      <div className="card">
        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>Código</th>
                <th>Ingrediente</th>
                <th>Proveedor</th>
                <th>Cantidad</th>
                <th>Fecha Vencimiento</th>
                <th>Precio</th>
                <th>Estado</th>
              </tr>
            </thead>
            <tbody>
              {lotes.map((lote) => (
                <tr key={lote.id}>
                  <td>
                    <strong>{lote.codigo}</strong>
                  </td>
                  <td>{lote.ingredienteNombre}</td>
                  <td>{lote.proveedorNombre}</td>
                  <td>{lote.cantidadDisponible}</td>
                  <td>
                    {format(new Date(lote.fechaVencimiento), "dd/MM/yyyy")}
                  </td>
                  <td>
                    ${lote.precioUnitario.toLocaleString()} {lote.moneda}
                  </td>
                  <td>
                    <span
                      className={`badge ${
                        lote.diasParaVencer < 7
                          ? "badge-danger"
                          : lote.diasParaVencer < 30
                          ? "badge-warning"
                          : "badge-success"
                      }`}
                    >
                      {lote.diasParaVencer} días
                    </span>
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

export default Lotes;
