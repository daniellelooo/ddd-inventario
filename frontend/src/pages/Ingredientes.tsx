import React, { useEffect, useState } from "react";
import { FiPlus, FiEdit, FiTrash2, FiSearch } from "react-icons/fi";
import { ingredientesService, categoriasService } from "../services/api";
import { Ingrediente } from "../types";
import "./CommonPages.css";

interface Categoria {
  id: string;
  nombre: string;
  descripcion: string;
}

const Ingredientes: React.FC = () => {
  const [ingredientes, setIngredientes] = useState<Ingrediente[]>([]);
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const [showModal, setShowModal] = useState(false);
  const [formData, setFormData] = useState({
    nombre: "",
    descripcion: "",
    unidadMedida: "kg",
    stockMinimo: 0,
    stockMaximo: 100,
    categoriaId: "",
  });

  useEffect(() => {
    loadIngredientes();
    loadCategorias();
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

  const loadCategorias = async () => {
    try {
      const data = await categoriasService.getAll();
      setCategorias(data);
    } catch (error) {
      console.error("Error cargando categorías:", error);
    }
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      // Crear objeto compatible con la API
      const ingredienteData = {
        nombre: formData.nombre,
        descripcion: formData.descripcion,
        unidadMedida: formData.unidadMedida,
        stockMinimo: formData.stockMinimo,
        stockMaximo: formData.stockMaximo,
        categoriaId: formData.categoriaId,
      };

      await ingredientesService.create(ingredienteData as any);
      alert("✅ Ingrediente creado exitosamente");
      setShowModal(false);
      setFormData({
        nombre: "",
        descripcion: "",
        unidadMedida: "kg",
        stockMinimo: 0,
        stockMaximo: 100,
        categoriaId: "",
      });
      loadIngredientes();
    } catch (error: any) {
      alert("❌ Error: " + (error.response?.data?.message || error.message));
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
        <button className="btn btn-primary" onClick={() => setShowModal(true)}>
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

      {/* Modal para crear ingrediente */}
      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>Nuevo Ingrediente</h2>
              <button className="btn-close" onClick={() => setShowModal(false)}>
                ×
              </button>
            </div>
            <form onSubmit={handleCreate}>
              <div className="form-group">
                <label>Nombre *</label>
                <input
                  type="text"
                  required
                  value={formData.nombre}
                  onChange={(e) =>
                    setFormData({ ...formData, nombre: e.target.value })
                  }
                  placeholder="Ej: Carne de res"
                />
              </div>
              <div className="form-group">
                <label>Descripción</label>
                <textarea
                  value={formData.descripcion}
                  onChange={(e) =>
                    setFormData({ ...formData, descripcion: e.target.value })
                  }
                  placeholder="Descripción del ingrediente"
                  rows={3}
                />
              </div>
              <div className="form-row">
                <div className="form-group">
                  <label>Categoría *</label>
                  <select
                    required
                    value={formData.categoriaId}
                    onChange={(e) =>
                      setFormData({ ...formData, categoriaId: e.target.value })
                    }
                  >
                    <option value="">Seleccione una categoría</option>
                    {categorias.map((cat) => (
                      <option key={cat.id} value={cat.id}>
                        {cat.nombre}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label>Unidad de Medida *</label>
                  <select
                    value={formData.unidadMedida}
                    onChange={(e) =>
                      setFormData({ ...formData, unidadMedida: e.target.value })
                    }
                  >
                    <option value="kg">Kilogramos (kg)</option>
                    <option value="g">Gramos (g)</option>
                    <option value="L">Litros (L)</option>
                    <option value="ml">Mililitros (ml)</option>
                    <option value="unidad">Unidades</option>
                  </select>
                </div>
              </div>
              <div className="form-row">
                <div className="form-group">
                  <label>Stock Mínimo *</label>
                  <input
                    type="number"
                    required
                    min="0"
                    step="0.01"
                    value={formData.stockMinimo}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        stockMinimo: parseFloat(e.target.value),
                      })
                    }
                  />
                </div>
                <div className="form-group">
                  <label>Stock Máximo *</label>
                  <input
                    type="number"
                    required
                    min="0"
                    step="0.01"
                    value={formData.stockMaximo}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        stockMaximo: parseFloat(e.target.value),
                      })
                    }
                  />
                </div>
              </div>
              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setShowModal(false)}
                >
                  Cancelar
                </button>
                <button type="submit" className="btn btn-primary">
                  Crear Ingrediente
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Ingredientes;
