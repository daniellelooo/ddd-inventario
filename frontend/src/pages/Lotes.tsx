import React, { useEffect, useState } from "react";
import { FiPlus, FiEdit, FiTrash2, FiCheck, FiX } from "react-icons/fi";
import { lotesService, ingredientesService, proveedoresService } from "../services/api";
import { Lote, Ingrediente, Proveedor } from "../types";
import "./CommonPages.css";
import { format } from "date-fns";

const Lotes: React.FC = () => {
  const [lotes, setLotes] = useState<Lote[]>([]);
  const [ingredientes, setIngredientes] = useState<Ingrediente[]>([]);
  const [proveedores, setProveedores] = useState<Proveedor[]>([]);
  const [loading, setLoading] = useState(true);
  const [form, setForm] = useState<Partial<Lote> | null>(null);
  const [editId, setEditId] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    loadLotes();
    ingredientesService.getAll().then(setIngredientes).catch(() => setIngredientes([]));
    proveedoresService.getAll().then(setProveedores).catch(() => setProveedores([]));
  }, []);

  const loadLotes = async () => {
    setLoading(true);
    try {
      const data = await lotesService.getAll();
      setLotes(data);
    } catch (error) {
      setLotes([]);
    } finally {
      setLoading(false);
    }
  };

  const handleAddClick = () => {
    setForm({
      codigo: "",
      ingredienteId: "",
      proveedorId: "",
      cantidadInicial: 0,
      fechaVencimiento: "",
    });
    setEditId(null);
  };

  const handleEditClick = (lote: Lote) => {
    setForm({ ...lote });
    setEditId(lote.id);
  };

  const handleCancel = () => {
    setForm(null);
    setEditId(null);
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    let checked: boolean | undefined = undefined;
    if (type === "checkbox" && 'checked' in e.target) {
      checked = (e.target as HTMLInputElement).checked;
    }
    setForm((prev) => {
      if (!prev) return prev;
      if (type === "checkbox") {
        return { ...prev, [name]: checked };
      }
      if (["cantidadInicial", "cantidadDisponible", "precioUnitario"].includes(name)) {
        return { ...prev, [name]: Number(value) };
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
        // No update endpoint in service, so skip
      } else {
        await lotesService.create(form);
      }
      await loadLotes();
      setForm(null);
      setEditId(null);
    } catch (err: any) {
      const msg = err?.message || err?.response?.data?.message || 'Error al guardar lote';
      alert(msg);
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("¿Eliminar este lote?")) return;
    setSaving(true);
    try {
      await lotesService.delete(id);
      await loadLotes();
    } catch (err) {
      alert("Error al eliminar lote");
    } finally {
      setSaving(false);
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
        <button className="btn btn-primary" onClick={handleAddClick} disabled={!!form}>
          <FiPlus /> Nuevo Lote
        </button>
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
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {form && (
                <tr>
                  <td>
                    <input
                      name="codigo"
                      value={form.codigo || ""}
                      onChange={handleChange}
                      placeholder="Ej: Lote-001"
                      required
                    />
                  </td>
                  <td>
                    <select
                      name="ingredienteId"
                      value={form.ingredienteId || ""}
                      onChange={handleChange}
                      required
                    >
                      <option value="">Selecciona ingrediente</option>
                      {ingredientes.map((ing) => (
                        <option key={ing.id} value={ing.id}>{ing.nombre}</option>
                      ))}
                    </select>
                  </td>
                  <td>
                    <select
                      name="proveedorId"
                      value={form.proveedorId || ""}
                      onChange={handleChange}
                      required
                    >
                      <option value="">Selecciona proveedor</option>
                      {proveedores.map((prov) => (
                        <option key={prov.id} value={prov.id}>{prov.nombre}</option>
                      ))}
                    </select>
                  </td>
                  <td>
                    <input
                      name="cantidadInicial"
                      type="number"
                      value={form.cantidadInicial ?? 0}
                      onChange={handleChange}
                      min={1}
                      step={1}
                      required
                    />
                  </td>
                  <td>
                    <input
                      name="fechaVencimiento"
                      type="date"
                      value={form.fechaVencimiento || ""}
                      onChange={handleChange}
                      required
                    />
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
              {lotes.map((lote) => (
                <tr key={lote.id}>
                  <td>
                    <strong>{lote.codigo}</strong>
                  </td>
                  <td>{lote.ingredienteNombre}</td>
                  <td>{lote.proveedorNombre}</td>
                  <td>{lote.cantidadDisponible}</td>
                  <td>{format(new Date(lote.fechaVencimiento), "dd/MM/yyyy")}</td>
                  <td>
                    <div className="action-buttons">
                      <button
                        className="btn-icon btn-icon-danger"
                        title="Eliminar"
                        onClick={() => handleDelete(lote.id)}
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

export default Lotes;
