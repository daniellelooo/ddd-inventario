import React, { useEffect, useState } from "react";
import { FiSearch, FiArrowUp, FiArrowDown } from "react-icons/fi";
import { movimientosService } from "../services/api";
import { MovimientoInventario } from "../types";
import "./CommonPages.css";

const Movimientos: React.FC = () => {
  const [movimientos, setMovimientos] = useState<MovimientoInventario[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");

  useEffect(() => {
    loadMovimientos();
  }, []);

  const loadMovimientos = async () => {
    try {
      setLoading(true);
      const data = await movimientosService.getAll();
      setMovimientos(data);
    } catch (error) {
      console.error("Error:", error);
      // Si no hay datos, mostramos array vacío
      setMovimientos([]);
    } finally {
      setLoading(false);
    }
  };

  const filteredMovimientos = movimientos.filter(
    (mov) =>
      mov.ingredienteNombre?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      mov.motivo?.toLowerCase().includes(searchTerm.toLowerCase())
  );

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
          <h1>Movimientos de Inventario</h1>
          <p>Historial de entradas y salidas</p>
        </div>
      </div>

      <div className="page-filters">
        <div className="search-box">
          <FiSearch />
          <input
            type="text"
            placeholder="Buscar movimiento..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
      </div>

      <div className="card">
        {filteredMovimientos.length === 0 ? (
          <p className="empty-state">
            {movimientos.length === 0
              ? "No hay movimientos registrados"
              : "No se encontraron movimientos"}
          </p>
        ) : (
          <div className="table-container">
            <table>
              <thead>
                <tr>
                  <th>Tipo</th>
                  <th>Ingrediente</th>
                  <th>Cantidad</th>
                  <th>Motivo</th>
                  <th>Fecha</th>
                  <th>Usuario</th>
                </tr>
              </thead>
              <tbody>
                {filteredMovimientos.map((mov) => (
                  <tr key={mov.id}>
                    <td>
                      {mov.tipoMovimiento === "Entrada" ? (
                        <span className="badge badge-success">
                          <FiArrowDown /> Entrada
                        </span>
                      ) : (
                        <span className="badge badge-danger">
                          <FiArrowUp /> Salida
                        </span>
                      )}
                    </td>
                    <td>
                      <strong>{mov.ingredienteNombre}</strong>
                    </td>
                    <td>
                      {mov.cantidad} {String(mov.unidadMedida)}
                    </td>
                    <td>{mov.motivo || "N/A"}</td>
                    <td>{new Date(mov.fechaMovimiento).toLocaleString()}</td>
                    <td>{mov.usuarioId || "Sistema"}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};

export default Movimientos;
