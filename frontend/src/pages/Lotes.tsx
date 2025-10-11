import React, { useEffect, useState } from "react";
import { FiSearch } from "react-icons/fi";
import { lotesService } from "../services/api";
import "./CommonPages.css";
import { format } from "date-fns";

interface LoteConInfo {
  id: string;
  codigo: string;
  ingredienteNombre: string;
  cantidadInicial: number;
  cantidadDisponible: number;
  fechaVencimiento: string;
  diasHastaVencimiento: number;
  precioUnitario: number;
  moneda: string;
}

const Lotes: React.FC = () => {
  const [lotes, setLotes] = useState<LoteConInfo[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadLotes();
  }, []);

  const loadLotes = async () => {
    try {
      // Get all lotes with ingredient names enriched
      const data = await lotesService.getAllWithDetails();
      setLotes(data);
      console.log("Lotes cargados:", data.length);
    } catch (error) {
      console.error("Error cargando lotes:", error);
      setLotes([]);
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
                <th>Cantidad</th>
                <th>Fecha Vencimiento</th>
                <th>Precio</th>
                <th>Estado</th>
              </tr>
            </thead>
            <tbody>
              {lotes.length === 0 ? (
                <tr>
                  <td
                    colSpan={6}
                    style={{ textAlign: "center", padding: "2rem" }}
                  >
                    No hay lotes registrados
                  </td>
                </tr>
              ) : (
                lotes.map((lote) => {
                  return (
                    <tr key={lote.id}>
                      <td>
                        <strong>{lote.codigo}</strong>
                      </td>
                      <td>{lote.ingredienteNombre || "N/A"}</td>
                      <td>
                        {lote.cantidadDisponible} / {lote.cantidadInicial}
                      </td>
                      <td>
                        {format(new Date(lote.fechaVencimiento), "dd/MM/yyyy")}
                      </td>
                      <td>
                        ${lote.precioUnitario?.toLocaleString() || "0"}{" "}
                        {lote.moneda || "COP"}
                      </td>
                      <td>
                        <span
                          className={`badge ${
                            lote.diasHastaVencimiento < 7
                              ? "badge-danger"
                              : lote.diasHastaVencimiento < 30
                              ? "badge-warning"
                              : "badge-success"
                          }`}
                        >
                          {lote.diasHastaVencimiento} días
                        </span>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default Lotes;
