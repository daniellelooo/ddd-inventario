import React, { useEffect, useState } from "react";
import { FiSearch } from "react-icons/fi";
import { movimientosService, ingredientesService } from "../services/api";
import { MovimientoInventario, TipoMovimiento, Ingrediente } from "../types";
import "./CommonPages.css";

const Movimientos: React.FC = () => {
  const [movimientos, setMovimientos] = useState<MovimientoInventario[]>([]);
  const [ingredientes, setIngredientes] = useState<Ingrediente[]>([]);
  const [ingredienteId, setIngredienteId] = useState<string>("");
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [tipo, setTipo] = useState<"Todos" | TipoMovimiento>("Todos");

  useEffect(() => {
    // cargar ingredientes para seleccionar
    (async () => {
      try {
        const list = await ingredientesService.getAll();
        setIngredientes(list);
        if (list.length > 0) {
          setIngredienteId(list[0].id);
        } else {
          // Si no hay ingredientes, terminar carga para mostrar estado vacío
          setLoading(false);
        }
      } catch {
        setIngredientes([]);
        setLoading(false);
      }
    })();
  }, []);

  useEffect(() => {
    if (!ingredienteId) return;
    loadMovimientos(ingredienteId);
  }, [ingredienteId]);

  const loadMovimientos = async (id: string) => {
    setLoading(true);
    try {
      // Historial por ingrediente desde backend
      if (!id) {
        setMovimientos([]);
        return;
      }
      const data = await movimientosService.getHistorial(id);
      setMovimientos(data);
    } catch (e) {
      setMovimientos([]);
    } finally {
      setLoading(false);
    }
  };

  const filtered = movimientos.filter((m) => {
    const matchesTipo = tipo === "Todos" ? true : m.tipoMovimiento === tipo;
    const term = search.toLowerCase();
    const matchesSearch =
      m.ingredienteNombre?.toLowerCase().includes(term) ||
      m.motivo?.toLowerCase().includes(term) ||
      m.loteCodigo?.toLowerCase().includes(term) ||
      false;
    return matchesTipo && matchesSearch;
  });

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
          <p>Entradas, salidas y ajustes</p>
        </div>
      </div>

      <div className="page-filters">
        <div>
          {ingredientes.length > 0 ? (
            <select value={ingredienteId} onChange={(e) => setIngredienteId(e.target.value)}>
              {ingredientes.map((ing) => (
                <option key={ing.id} value={ing.id}>{ing.nombre}</option>
              ))}
            </select>
          ) : (
            <span style={{ color: '#64748b' }}>No hay ingredientes disponibles</span>
          )}
        </div>
        <div className="search-box">
          <FiSearch />
          <input
            type="text"
            placeholder="Buscar por ingrediente, lote o motivo..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
        <div>
          <select value={tipo} onChange={(e) => setTipo(e.target.value as any)}>
            <option value="Todos">Todos</option>
            <option value={TipoMovimiento.Entrada}>Entradas</option>
            <option value={TipoMovimiento.Salida}>Salidas</option>
            <option value={TipoMovimiento.Ajuste}>Ajustes</option>
          </select>
        </div>
      </div>

      <div className="card">
        {filtered.length === 0 ? (
          <p className="empty-state">No hay movimientos para los filtros seleccionados</p>
        ) : (
          <div className="table-container">
            <table>
              <thead>
                <tr>
                  <th>Fecha</th>
                  <th>Ingrediente</th>
                  <th>Lote</th>
                  <th>Tipo</th>
                  <th>Cantidad</th>
                  <th>Unidad</th>
                  <th>Motivo</th>
                  <th>Documento</th>
                </tr>
              </thead>
              <tbody>
                {filtered.map((m) => (
                  <tr key={`${m.id}`}>
                    <td>{new Date(m.fechaMovimiento).toLocaleString()}</td>
                    <td>{m.ingredienteNombre || m.ingredienteId}</td>
                    <td>{m.loteCodigo || m.loteId || "-"}</td>
                    <td>
                      <span className={`badge ${
                        m.tipoMovimiento === TipoMovimiento.Entrada
                          ? "badge-success"
                          : m.tipoMovimiento === TipoMovimiento.Salida
                          ? "badge-danger"
                          : "badge-info"
                      }`}>
                        {m.tipoMovimiento}
                      </span>
                    </td>
                    <td>{m.cantidad}</td>
                    <td>{m.unidadMedida?.simbolo || m.unidadMedida?.nombre || ""}</td>
                    <td>{m.motivo}</td>
                    <td>{m.documentoReferencia || "-"}</td>
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
