import React, { useEffect, useState } from "react";
import { FiPlus, FiEdit, FiTrash2, FiSearch } from "react-icons/fi";
import { ingredientesService } from "../services/api";
import { Ingrediente } from "../types";
import "./CommonPages.css";

const Ingredientes: React.FC = () => {
  const [ingredientes, setIngredientes] = useState<Ingrediente[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");

  useEffect(() => {
    loadIngredientes();
  }, []);

  const loadIngredientes = async () => {
    try {
      setLoading(true);
      const data = await ingredientesService.getAll();
      setIngredientes(data);
    } catch (error) {
      console.error("Error:", error);
    } finally {
      setLoading(false);
    }
  };

  const filteredIngredientes = ingredientes.filter((ing) =>
    ing.nombre.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getStockStatus = (ing: Ingrediente) => {
    if (ing.cantidadEnStock < ing.stockMinimo) return "bajo";
    if (ing.cantidadEnStock > ing.stockMaximo) return "exceso";
    return "normal";
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
          <h1>Ingredientes</h1>
          <p>Gestión de ingredientes del inventario</p>
        </div>
        <button className="btn btn-primary">
          <FiPlus /> Nuevo Ingrediente
        </button>
      </div>

      <div className="page-filters">
        <div className="search-box">
          <FiSearch />
          <input
            type="text"
            placeholder="Buscar ingrediente..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
      </div>

      <div className="card">
        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Categoría</th>
                <th>Unidad</th>
                <th>Stock Actual</th>
                <th>Rango de Stock</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {filteredIngredientes.map((ing) => {
                const status = getStockStatus(ing);
                return (
                  <tr key={ing.id}>
                    <td>
                      <strong>{ing.nombre}</strong>
                    </td>
                    <td>{ing.categoriaNombre || "Sin categoría"}</td>
                    <td>
                      {ing.unidadMedida.nombre} ({ing.unidadMedida.simbolo})
                    </td>
                    <td>
                      <strong>{ing.cantidadEnStock}</strong>
                    </td>
                    <td>
                      {ing.stockMinimo} - {ing.stockMaximo}
                    </td>
                    <td>
                      <span
                        className={`badge badge-${
                          status === "bajo"
                            ? "danger"
                            : status === "exceso"
                            ? "warning"
                            : "success"
                        }`}
                      >
                        {status === "bajo"
                          ? "Bajo Stock"
                          : status === "exceso"
                          ? "Exceso"
                          : "Normal"}
                      </span>
                    </td>
                    <td>
                      <div className="action-buttons">
                        <button className="btn-icon" title="Editar">
                          <FiEdit />
                        </button>
                        <button
                          className="btn-icon btn-icon-danger"
                          title="Eliminar"
                        >
                          <FiTrash2 />
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default Ingredientes;
