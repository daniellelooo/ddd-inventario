import React, { useEffect, useState } from "react";
import { FiPlus, FiEdit, FiTrash2, FiCheck, FiX } from "react-icons/fi";
import { proveedoresService } from "../services/api";
import { Proveedor } from "../types";
import "./CommonPages.css";

const Proveedores: React.FC = () => {
  const [proveedores, setProveedores] = useState<Proveedor[]>([]);
  const [loading, setLoading] = useState(true);
  const [form, setForm] = useState<Partial<Proveedor> | null>(null);
  const [editId, setEditId] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    loadProveedores();
  }, []);

  const loadProveedores = async () => {
    setLoading(true);
    try {
      const data = await proveedoresService.getAll();
      setProveedores(data);
    } catch (error) {
      setProveedores([]);
    } finally {
      setLoading(false);
    }
  };

  const handleAddClick = () => {
    setForm({
      nombre: "",
      nit: "",
      telefono: "",
      email: "",
      direccion: { calle: "", ciudad: "", pais: "" },
      personaContacto: "",
      activo: true,
    });
    setEditId(null);
  };

  const handleEditClick = (prov: Proveedor) => {
    setForm({ ...prov, direccion: { ...prov.direccion } });
    setEditId(prov.id);
  };

  const handleCancel = () => {
    setForm(null);
    setEditId(null);
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const target = e.target;
    const name = target.name;
    const value = target.value;
    const type = target.type;
    const checked = type === "checkbox" ? target.checked : undefined;
    setForm((prev) => {
      if (!prev) return prev;
      // Always ensure direccion fields are present
      const direccion = {
        calle: prev.direccion?.calle ?? "",
        ciudad: prev.direccion?.ciudad ?? "",
        pais: prev.direccion?.pais ?? "",
        codigoPostal: prev.direccion?.codigoPostal ?? "",
      };
      if (name.startsWith("direccion.")) {
        return {
          ...prev,
          direccion: {
            ...direccion,
            [name.split(".")[1]]: value,
          },
        };
      }
      if (type === "checkbox") {
        return { ...prev, [name]: checked, direccion };
      }
      return { ...prev, [name]: value, direccion };
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form) return;
    setSaving(true);
    // El backend espera los campos de dirección como propiedades planas
    const payload = {
      nombre: form.nombre,
      nit: form.nit,
      telefono: form.telefono,
      email: form.email,
      calle: form.direccion?.calle || "",
      ciudad: form.direccion?.ciudad || "",
      pais: form.direccion?.pais || "",
      codigoPostal: form.direccion?.codigoPostal || "",
      personaContacto: form.personaContacto,
      activo: form.activo !== undefined ? form.activo : true
    };
    try {
      if (editId) {
        await proveedoresService.update(editId, payload);
      } else {
        await proveedoresService.create(payload);
      }
      await loadProveedores();
      setForm(null);
      setEditId(null);
    } catch (err) {
      const msg = (err as any)?.response?.data?.message || (err as any)?.message || "Error al guardar proveedor";
      alert(msg);
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("¿Eliminar este proveedor?")) return;
    setSaving(true);
    try {
      await proveedoresService.delete(id);
      await loadProveedores();
    } catch (err) {
      alert("Error al eliminar proveedor");
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
          <h1>Proveedores</h1>
          <p>Gestión de proveedores</p>
        </div>
        <button className="btn btn-primary" onClick={handleAddClick} disabled={!!form}>
          <FiPlus /> Nuevo Proveedor
        </button>
      </div>
      <div className="card">
        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>Nombre</th>
                <th>NIT</th>
                <th>Email</th>
                <th>Teléfono</th>
                <th>Calle</th>
                <th>Ciudad</th>
                <th>País</th>
                <th>Contacto</th>
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
                      placeholder="Ej: Proveedor S.A."
                      required
                    />
                  </td>
                  <td>
                    <input
                      name="nit"
                      value={form.nit || ""}
                      onChange={handleChange}
                      placeholder="NIT"
                      required
                    />
                  </td>
                  <td>
                    <input
                      name="email"
                      value={form.email || ""}
                      onChange={handleChange}
                      placeholder="Correo electrónico"
                      required
                    />
                  </td>
                  <td>
                    <input
                      name="telefono"
                      value={form.telefono || ""}
                      onChange={handleChange}
                      placeholder="Teléfono"
                      required
                    />
                  </td>
                  <td>
                    <input
                      name="direccion.calle"
                      value={form.direccion?.calle || ""}
                      onChange={handleChange}
                      placeholder="Calle y número"
                      required
                    />
                  </td>
                  <td>
                    <input
                      name="direccion.ciudad"
                      value={form.direccion?.ciudad || ""}
                      onChange={handleChange}
                      placeholder="Ciudad"
                      required
                    />
                  </td>
                  <td>
                    <input
                      name="direccion.pais"
                      value={form.direccion?.pais || ""}
                      onChange={handleChange}
                      placeholder="País"
                      required
                    />
                  </td>
                  <td>
                    <input
                      name="personaContacto"
                      value={form.personaContacto || ""}
                      onChange={handleChange}
                      placeholder="Persona de contacto"
                      required
                    />
                  </td>
                  <td>
                    <label style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                      <input
                        type="checkbox"
                        name="activo"
                        checked={!!form.activo}
                        onChange={handleChange}
                      />
                      Activo
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
              {proveedores.map((prov) => (
                <tr key={prov.id}>
                  <td>{prov.nombre}</td>
                  <td>{prov.nit}</td>
                  <td>{prov.email}</td>
                  <td>{prov.telefono}</td>
                  <td>{prov.direccion.calle}</td>
                  <td>{prov.direccion.ciudad}</td>
                  <td>{prov.direccion.pais}</td>
                  <td>{prov.personaContacto}</td>
                  <td>
                    <span className={`badge ${prov.activo ? "badge-success" : "badge-secondary"}`}>
                      {prov.activo ? "Activo" : "Inactivo"}
                    </span>
                  </td>
                  <td>
                    <div className="action-buttons">
                      <button
                        className="btn-icon"
                        title="Editar"
                        onClick={() => handleEditClick(prov)}
                        disabled={!!form}
                      >
                        <FiEdit />
                      </button>
                      <button
                        className="btn-icon btn-icon-danger"
                        title="Eliminar"
                        onClick={() => handleDelete(prov.id)}
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

export default Proveedores;
