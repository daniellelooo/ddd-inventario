import React, { useEffect, useState } from "react";
import { FiPlus, FiEdit, FiTrash2, FiSearch, FiCheck, FiX } from "react-icons/fi";
import { ingredientesService, categoriasService } from "../services/api";
import { Ingrediente, Categoria } from "../types";
import "./CommonPages.css";

const Ingredientes: React.FC = () => {
  const [ingredientes, setIngredientes] = useState<Ingrediente[]>([]);
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const [form, setForm] = useState<Partial<Ingrediente> | null>(null);
  const [editId, setEditId] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);


  useEffect(() => {
    loadIngredientes();
    loadCategorias();
  }, []);

  const loadCategorias = async () => {
    try {
      const data = await categoriasService.getAll();
      setCategorias(data);
    } catch (e) {
      setCategorias([]);
    }
  };
  const handleAddClick = () => {
    setForm({
      nombre: "",
      descripcion: "",
      categoriaId: categorias[0]?.id || "",
      unidadMedida: { nombre: "", simbolo: "" },
      stockMinimo: 1,
      stockMaximo: 10,
      activo: true
    });
    setEditId(null);
  };

  const handleEditClick = (ing: Ingrediente) => {
    setForm({ ...ing });
    setEditId(ing.id);
  };

  const handleCancel = () => {
    setForm(null);
    setEditId(null);
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    setForm((prev) => {
      if (!prev) return prev;
      if (type === "checkbox" && e.target instanceof HTMLInputElement) {
        return { ...prev, [name]: e.target.checked };
      }
      // Para stockMinimo y stockMaximo convertir a número
      if (name === "stockMinimo" || name === "stockMaximo") {
        return { ...prev, [name]: Number(value) };
      }
      return { ...prev, [name]: value };
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form) return;
    setSaving(true);
    // El backend espera unidadMedida como string (nombre o simbolo)
    const payload = {
      nombre: form.nombre,
      descripcion: form.descripcion || "",
      categoriaId: form.categoriaId,
      unidadMedida: form.unidadMedida?.nombre || "",
      stockMinimo: form.stockMinimo,
      stockMaximo: form.stockMaximo,
      activo: form.activo !== undefined ? form.activo : true
    } as any; // for API, unidadMedida is string
    try {
      if (editId) {
        await ingredientesService.update(editId, payload);
      } else {
        await ingredientesService.create(payload);
      }
      await loadIngredientes();
      setForm(null);
      setEditId(null);
    } catch (err: any) {
      if (err?.response?.data?.message) {
        alert("Error: " + err.response.data.message);
      } else {
        alert("Error al guardar ingrediente");
      }
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("¿Eliminar este ingrediente?")) return;
    setSaving(true);
    try {
      await ingredientesService.delete(id);
      await loadIngredientes();
    } catch (err) {
      alert("Error al eliminar ingrediente");
    } finally {
      setSaving(false);
    }
  };

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
        <button className="btn btn-primary" onClick={handleAddClick} disabled={!!form}>
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
                <th title="Unidad de medida principal">Unidad <span style={{color:'#64748b',fontWeight:400}}>(?)</span></th>
                <th title="Cantidad actual en inventario">Stock <span style={{color:'#64748b',fontWeight:400}}>(?)</span></th>
                <th title="Rango recomendado de stock">Rango <span style={{color:'#64748b',fontWeight:400}}>(?)</span></th>
                <th title="Estado del stock según el rango">Estado <span style={{color:'#64748b',fontWeight:400}}>(?)</span></th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {form && (
                <tr>
                  <td colSpan={7} style={{ background: '#f9fafb' }}>
                    <form onSubmit={handleSubmit} style={{ display: 'flex', gap: 16, alignItems: 'center', flexWrap: 'wrap' }}>
                      <div style={{ minWidth: 180 }}>
                        <input
                          name="nombre"
                          value={form.nombre || ""}
                          onChange={handleChange}
                          placeholder="Ej: Tomate, Harina, Aceite..."
                          required
                          autoFocus
                          style={{ width: '100%' }}
                        />
                      </div>
                      <div style={{ minWidth: 120 }}>
                        <input
                          name="descripcion"
                          value={form.descripcion || ""}
                          onChange={handleChange}
                          placeholder="Descripción"
                          style={{ width: '100%' }}
                        />
                      </div>
                      <div style={{ minWidth: 140 }}>
                        <select
                          name="categoriaId"
                          value={form.categoriaId || ""}
                          onChange={handleChange}
                          required
                          style={{ width: '100%' }}
                        >
                          <option value="">Selecciona categoría</option>
                          {categorias.map((cat) => (
                            <option key={cat.id} value={cat.id}>
                              {cat.nombre}
                            </option>
                          ))}
                        </select>
                      </div>
                      <div style={{ minWidth: 140 }}>
                        {(() => {
                          const commonUnits = [
                            { nombre: "Kilogramo", simbolo: "kg" },
                            { nombre: "Gramo", simbolo: "g" },
                            { nombre: "Litro", simbolo: "l" },
                            { nombre: "Mililitro", simbolo: "ml" },
                            { nombre: "Unidad", simbolo: "u" },
                          ];
                          return (
                            <select
                              name="unidadMedida"
                              value={form.unidadMedida?.nombre || ""}
                              onChange={(e) => {
                                const sel = commonUnits.find(u => u.nombre === e.target.value) || { nombre: e.target.value, simbolo: "" };
                                setForm((prev) => prev ? { ...prev, unidadMedida: sel } : prev);
                              }}
                              required
                              style={{ width: '100%' }}
                            >
                              <option value="">Selecciona unidad</option>
                              {commonUnits.map((u) => (
                                <option key={u.simbolo} value={u.nombre}>{`${u.nombre} (${u.simbolo})`}</option>
                              ))}
                            </select>
                          );
                        })()}
                      </div>
                      <div style={{ minWidth: 90 }}>
                        <input
                          name="stockMinimo"
                          type="number"
                          min={0}
                          value={form.stockMinimo ?? 1}
                          onChange={handleChange}
                          placeholder="Stock mín."
                          required
                          style={{ width: '100%' }}
                        />
                      </div>
                      <div style={{ minWidth: 90 }}>
                        <input
                          name="stockMaximo"
                          type="number"
                          min={1}
                          value={form.stockMaximo ?? 10}
                          onChange={handleChange}
                          placeholder="Stock máx."
                          required
                          style={{ width: '100%' }}
                        />
                      </div>
                      <div style={{ minWidth: 80, display: 'flex', alignItems: 'center', gap: 4 }}>
                        <label style={{ fontSize: 13 }}>
                          <input
                            type="checkbox"
                            name="activo"
                            checked={form.activo ?? true}
                            onChange={handleChange}
                            style={{ marginRight: 4 }}
                          />Activo
                        </label>
                      </div>
                      <div className="action-buttons">
                        <button
                          className="btn-icon btn-icon-success"
                          title="Guardar"
                          type="submit"
                          disabled={saving}
                        >
                          <FiCheck />
                        </button>
                        <button
                          className="btn-icon btn-icon-danger"
                          title="Cancelar"
                          type="button"
                          onClick={handleCancel}
                          disabled={saving}
                        >
                          <FiX />
                        </button>
                      </div>
                    </form>
                  </td>
                </tr>
              )}
              {filteredIngredientes.map((ing) => {
                const status = getStockStatus(ing);
                return (
                  <tr key={ing.id}>
                    <td>
                      <strong>{ing.nombre}</strong>
                    </td>
                    <td>{ing.categoriaNombre || <span style={{color:'#64748b'}}>Sin categoría</span>}</td>
                    <td title={ing.unidadMedida.simbolo ? `Unidad: ${ing.unidadMedida.nombre} (${ing.unidadMedida.simbolo})` : ''}>
                      {ing.unidadMedida.nombre} <span style={{color:'#64748b'}}>({ing.unidadMedida.simbolo})</span>
                    </td>
                    <td title="Cantidad actual en inventario">
                      <strong>{ing.cantidadEnStock}</strong>
                    </td>
                    <td title="Rango recomendado de stock">
                      <span>{ing.stockMinimo} - {ing.stockMaximo}</span>
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
                        title={status === "bajo"
                          ? "El stock está por debajo del mínimo recomendado"
                          : status === "exceso"
                          ? "El stock supera el máximo recomendado"
                          : "El stock está dentro del rango recomendado"}
                      >
                        {status === "bajo"
                          ? "Bajo"
                          : status === "exceso"
                          ? "Exceso"
                          : "Normal"}
                      </span>
                    </td>
                    <td>
                      <div className="action-buttons">
                        <button
                          className="btn-icon"
                          title="Editar"
                          onClick={() => handleEditClick(ing)}
                          disabled={!!form}
                        >
                          <FiEdit />
                        </button>
                        <button
                          className="btn-icon btn-icon-danger"
                          title="Eliminar"
                          onClick={() => handleDelete(ing.id)}
                          disabled={saving}
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
