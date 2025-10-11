import React, { useEffect, useState } from "react";
import { FiPlus, FiEdit, FiTrash2 } from "react-icons/fi";
import { categoriasService } from "../services/api";
import "./CommonPages.css";

interface Categoria {
  id: string;
  nombre: string;
  descripcion: string;
  activa: boolean;
  fechaCreacion: string;
}

const Categorias: React.FC = () => {
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [formData, setFormData] = useState({
    nombre: "",
    descripcion: "",
  });

  useEffect(() => {
    loadCategorias();
  }, []);

  const loadCategorias = async () => {
    try {
      setLoading(true);
      const data = await categoriasService.getAll();
      setCategorias(data);
    } catch (error) {
      console.error("Error:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await categoriasService.create(formData as any);
      alert("✅ Categoría creada exitosamente");
      setShowModal(false);
      setFormData({ nombre: "", descripcion: "" });
      loadCategorias();
    } catch (error: any) {
      alert("❌ Error: " + (error.response?.data?.message || error.message));
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
          <h1>Categorías</h1>
          <p>Gestión de categorías de ingredientes</p>
        </div>
        <button className="btn btn-primary" onClick={() => setShowModal(true)}>
          <FiPlus /> Nueva Categoría
        </button>
      </div>

      <div className="card">
        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Descripción</th>
                <th>Estado</th>
                <th>Fecha Creación</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {categorias.map((cat) => (
                <tr key={cat.id}>
                  <td>
                    <strong>{cat.nombre}</strong>
                  </td>
                  <td>{cat.descripcion}</td>
                  <td>
                    <span
                      className={`badge badge-${
                        cat.activa ? "success" : "danger"
                      }`}
                    >
                      {cat.activa ? "Activa" : "Inactiva"}
                    </span>
                  </td>
                  <td>{new Date(cat.fechaCreacion).toLocaleDateString()}</td>
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
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Modal para crear categoría */}
      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>Nueva Categoría</h2>
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
                  placeholder="Ej: Carnes"
                />
              </div>
              <div className="form-group">
                <label>Descripción</label>
                <textarea
                  value={formData.descripcion}
                  onChange={(e) =>
                    setFormData({ ...formData, descripcion: e.target.value })
                  }
                  placeholder="Descripción de la categoría"
                  rows={3}
                />
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
                  Crear Categoría
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Categorias;
