import React, { useEffect, useState } from "react";
import { FiPlus, FiEdit, FiTrash2, FiCheck, FiX } from "react-icons/fi";
import { categoriasService } from "../services/api";
import { Categoria } from "../types";
import "./CommonPages.css";

const Categorias: React.FC = () => {
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [loading, setLoading] = useState(true);
  const [form, setForm] = useState<Partial<Categoria> | null>(null);
  const [editId, setEditId] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    loadCategorias();
  }, []);

  const loadCategorias = async () => {
    setLoading(true);
    try {
      const data = await categoriasService.getAll();
      setCategorias(data);
    } catch (e) {
      setCategorias([]);
    } finally {
      setLoading(false);
    }
  };

  const handleAddClick = () => {
    setForm({ nombre: "", descripcion: "", activa: true });
    setEditId(null);
  };

  const handleEditClick = (cat: Categoria) => {
    setForm({ ...cat });
    setEditId(cat.id);
  };

  const handleCancel = () => {
    setForm(null);
    setEditId(null);
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const target = e.target;
    const name = target.name;
    const value = target.value;
    const type = target.type;
    const checked = (target instanceof HTMLInputElement && type === "checkbox") ? target.checked : false;
    setForm((prev) => {
      if (!prev) return prev;
      if (type === "checkbox") {
        return { ...prev, [name]: checked };
      }
      return { ...prev, [name]: value };
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form) return;
    setSaving(true);
    try {
      if (editId) {
        await categoriasService.update(editId, form);
      } else {
        await categoriasService.create(form);
      }
      await loadCategorias();
      setForm(null);
      setEditId(null);
    } catch (err) {
      alert("Error al guardar categoría");
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("¿Eliminar esta categoría?")) return;
    setSaving(true);
    try {
      await categoriasService.delete(id);
      await loadCategorias();
    } catch (err) {
      alert("Error al eliminar categoría");
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Categorías</h1>
          <p>Gestión de categorías de ingredientes</p>
        </div>
        <button className="btn btn-primary" onClick={handleAddClick} disabled={!!form}>
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
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {form && (
                <tr>
                  <td>
                    <input
                      name="nombre"
                      value={form.nombre || ""}
                      onChange={handleChange}
                      placeholder="Nombre de la categoría"
                      required
                      autoFocus
                    />
                  </td>
                  <td>
                    <input
                      name="descripcion"
                      value={form.descripcion || ""}
                      onChange={handleChange}
                      placeholder="Descripción (opcional)"
                    />
                  </td>
                  <td>
                    <label style={{ display: "flex", alignItems: "center", gap: 4 }}>
                      <input
                        type="checkbox"
                        name="activa"
                        checked={!!form.activa}
                        onChange={handleChange}
                        style={{ marginRight: 4 }}
                      />
                      {form.activa ? "Activa" : "Inactiva"}
                    </label>
                  </td>
                  <td>
                    <div className="action-buttons">
                      <button
                        className="btn-icon btn-icon-success"
                        title="Guardar"
                        onClick={handleSubmit}
                        disabled={saving}
                      >
                        <FiCheck />
                      </button>
                      <button
                        className="btn-icon btn-icon-danger"
                        title="Cancelar"
                        onClick={handleCancel}
                        disabled={saving}
                      >
                        <FiX />
                      </button>
                    </div>
                  </td>
                </tr>
              )}
              {categorias.map((cat) => (
                <tr key={cat.id}>
                  <td>
                    <strong>{cat.nombre}</strong>
                  </td>
                  <td>{cat.descripcion}</td>
                  <td>
                    <span className={`badge ${cat.activa ? "badge-success" : "badge-secondary"}`}>
                      {cat.activa ? "Activa" : "Inactiva"}
                    </span>
                  </td>
                  <td>
                    <div className="action-buttons">
                      <button
                        className="btn-icon"
                        title="Editar"
                        onClick={() => handleEditClick(cat)}
                        disabled={!!form}
                      >
                        <FiEdit />
                      </button>
                      <button
                        className="btn-icon btn-icon-danger"
                        title="Eliminar"
                        onClick={() => handleDelete(cat.id)}
                        disabled={saving}
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
    </div>
  );
};

export default Categorias;
