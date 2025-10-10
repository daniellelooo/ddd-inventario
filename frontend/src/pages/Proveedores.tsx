import React, { useEffect, useState } from "react";
import { FiPlus } from "react-icons/fi";
import { proveedoresService } from "../services/api";
import { Proveedor } from "../types";
import "./CommonPages.css";

const Proveedores: React.FC = () => {
  const [proveedores, setProveedores] = useState<Proveedor[]>([]);
  const [loading, setLoading] = useState(true);

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
        <button className="btn btn-primary">
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
    </div>
  );
};

export default Proveedores;
