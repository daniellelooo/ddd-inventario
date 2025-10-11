import React, { useEffect, useState } from "react";
import { FiPlus, FiX } from "react-icons/fi";
import { proveedoresService } from "../services/api";
import { Proveedor } from "../types";
import "./CommonPages.css";

const Proveedores: React.FC = () => {
  const [proveedores, setProveedores] = useState<Proveedor[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);

  const [newProveedor, setNewProveedor] = useState({
    nombre: "",
    nit: "",
    telefono: "",
    email: "",
    direccion: {
      calle: "",
      ciudad: "",
      pais: "Colombia",
      codigoPostal: "",
    },
    personaContacto: "",
  });

  useEffect(() => {
    loadProveedores();
  }, []);

  const loadProveedores = async () => {
    try {
      const data = await proveedoresService.getAll();
      setProveedores(data);
    } catch (error) {
      console.error("Error:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = async () => {
    try {
      await proveedoresService.create(newProveedor as any);
      setShowModal(false);
      setNewProveedor({
        nombre: "",
        nit: "",
        telefono: "",
        email: "",
        direccion: {
          calle: "",
          ciudad: "",
          pais: "Colombia",
          codigoPostal: "",
        },
        personaContacto: "",
      });
      await loadProveedores();
    } catch (error) {
      console.error("Error:", error);
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
        <button className="btn btn-primary" onClick={() => setShowModal(true)}>
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
                <th>Ciudad</th>
                <th>Contacto</th>
                <th>Estado</th>
              </tr>
            </thead>
            <tbody>
              {proveedores.map((prov) => (
                <tr key={prov.id}>
                  <td>
                    <strong>{prov.nombre}</strong>
                  </td>
                  <td>{prov.nit}</td>
                  <td>{prov.email}</td>
                  <td>{prov.telefono}</td>
                  <td>
                    {prov.direccion.ciudad}, {prov.direccion.pais}
                  </td>
                  <td>{prov.personaContacto}</td>
                  <td>
                    <span
                      className={`badge ${
                        prov.activo ? "badge-success" : "badge-secondary"
                      }`}
                    >
                      {prov.activo ? "Activo" : "Inactivo"}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>Nuevo Proveedor</h2>
              <button className="close-btn" onClick={() => setShowModal(false)}>
                <FiX />
              </button>
            </div>
            <div className="modal-body">
              <div className="form-row">
                <div className="form-group">
                  <label>Nombre *</label>
                  <input
                    type="text"
                    value={newProveedor.nombre}
                    onChange={(e) =>
                      setNewProveedor({
                        ...newProveedor,
                        nombre: e.target.value,
                      })
                    }
                    required
                  />
                </div>
                <div className="form-group">
                  <label>NIT *</label>
                  <input
                    type="text"
                    value={newProveedor.nit}
                    onChange={(e) =>
                      setNewProveedor({ ...newProveedor, nit: e.target.value })
                    }
                    required
                  />
                </div>
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>Email *</label>
                  <input
                    type="email"
                    value={newProveedor.email}
                    onChange={(e) =>
                      setNewProveedor({
                        ...newProveedor,
                        email: e.target.value,
                      })
                    }
                    required
                  />
                </div>
                <div className="form-group">
                  <label>Teléfono *</label>
                  <input
                    type="tel"
                    value={newProveedor.telefono}
                    onChange={(e) =>
                      setNewProveedor({
                        ...newProveedor,
                        telefono: e.target.value,
                      })
                    }
                    required
                  />
                </div>
              </div>

              <div className="form-group">
                <label>Persona Contacto *</label>
                <input
                  type="text"
                  value={newProveedor.personaContacto}
                  onChange={(e) =>
                    setNewProveedor({
                      ...newProveedor,
                      personaContacto: e.target.value,
                    })
                  }
                  required
                />
              </div>

              <div className="form-group">
                <label>Dirección *</label>
                <input
                  type="text"
                  value={newProveedor.direccion.calle}
                  onChange={(e) =>
                    setNewProveedor({
                      ...newProveedor,
                      direccion: {
                        ...newProveedor.direccion,
                        calle: e.target.value,
                      },
                    })
                  }
                  placeholder="Calle y número"
                  required
                />
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>Ciudad *</label>
                  <input
                    type="text"
                    value={newProveedor.direccion.ciudad}
                    onChange={(e) =>
                      setNewProveedor({
                        ...newProveedor,
                        direccion: {
                          ...newProveedor.direccion,
                          ciudad: e.target.value,
                        },
                      })
                    }
                    required
                  />
                </div>
                <div className="form-group">
                  <label>País *</label>
                  <input
                    type="text"
                    value={newProveedor.direccion.pais}
                    onChange={(e) =>
                      setNewProveedor({
                        ...newProveedor,
                        direccion: {
                          ...newProveedor.direccion,
                          pais: e.target.value,
                        },
                      })
                    }
                    required
                  />
                </div>
              </div>

              <div className="form-group">
                <label>Código Postal</label>
                <input
                  type="text"
                  value={newProveedor.direccion.codigoPostal}
                  onChange={(e) =>
                    setNewProveedor({
                      ...newProveedor,
                      direccion: {
                        ...newProveedor.direccion,
                        codigoPostal: e.target.value,
                      },
                    })
                  }
                />
              </div>
            </div>
            <div className="modal-footer">
              <button
                className="btn btn-secondary"
                onClick={() => setShowModal(false)}
              >
                Cancelar
              </button>
              <button className="btn btn-primary" onClick={handleCreate}>
                Crear Proveedor
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Proveedores;
