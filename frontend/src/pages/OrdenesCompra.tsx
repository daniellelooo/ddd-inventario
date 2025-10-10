import React, { useEffect, useMemo, useState } from "react";
import { FiPlus, FiCheck, FiX } from "react-icons/fi";
import { ordenesCompraService, proveedoresService, ingredientesService } from "../services/api";
import { OrdenDeCompra, CrearOrdenDeCompraCommand, Proveedor, Ingrediente } from "../types";
import "./CommonPages.css";
import { format } from "date-fns";

const OrdenesCompra: React.FC = () => {
  const [ordenes, setOrdenes] = useState<OrdenDeCompra[]>([]);
  const [proveedores, setProveedores] = useState<Proveedor[]>([]);
  const [ingredientes, setIngredientes] = useState<Ingrediente[]>([]);
  const [loading, setLoading] = useState(true);
  const [form, setForm] = useState<Partial<CrearOrdenDeCompraCommand> | null>(null);
  const [saving, setSaving] = useState(false);
  const [receiveForId, setReceiveForId] = useState<string | null>(null);
  const [receiveData, setReceiveData] = useState<{ codigoLote: string; fechaVencimiento: string; observaciones?: string }>({ codigoLote: "", fechaVencimiento: "", observaciones: "" });

  // Lista enriquecida con nombres calculados desde proveedores/ingredientes
  const displayOrdenes = useMemo(() => {
    const provIndex = new Map<string, Proveedor>();
    proveedores.forEach((p) => provIndex.set(p.id, p));
    const ingIndex = new Map<string, Ingrediente>();
    ingredientes.forEach((i) => ingIndex.set(i.id, i));

    return (ordenes || []).map((ord) => {
      const proveedorId = (ord as any).proveedorId || (ord as any)?.ProveedorId;
      const prov = proveedorId ? provIndex.get(proveedorId) : undefined;
      const detalles = (ord as any).detalles || (ord as any).Detalles || [];
      const det0 = Array.isArray(detalles) ? detalles[0] : undefined;
      const ingredienteId = det0?.ingredienteId || det0?.IngredienteId || (ord as any).ingredienteId || (ord as any).IngredienteId;
      const ing = ingredienteId ? ingIndex.get(ingredienteId) : undefined;

      return {
        ...ord,
        proveedorNombre: prov?.nombre || (ord as any).proveedorNombre || "—",
        ingredienteNombre: ing?.nombre || (ord as any).ingredienteNombre || "—",
        cantidad: det0?.cantidad ?? (ord as any).cantidad ?? 0,
      } as OrdenDeCompra;
    });
  }, [ordenes, proveedores, ingredientes]);

  const loadOrdenes = async () => {
    try {
      const data = await ordenesCompraService.getPendientes();
      setOrdenes(data);
    } catch (error) {
      setOrdenes([]);
    } finally {
      setLoading(false);
    }
  };
  const loadProveedores = async () => {
    try {
      const data = await proveedoresService.getAll();
      setProveedores(data);
    } catch (e) {
      setProveedores([]);
    }
  };
  const loadIngredientes = async () => {
    try {
      const data = await ingredientesService.getAll();
      setIngredientes(data);
    } catch (e) {
      setIngredientes([]);
    }
  };

  useEffect(() => {
    loadOrdenes();
    loadProveedores();
    loadIngredientes();
  }, []);

  const handleAddClick = () => {
    setForm({
      proveedorId: proveedores[0]?.id || "",
      detalles: [{ ingredienteId: ingredientes[0]?.id || "", cantidad: 1, precioUnitario: 0, moneda: "BOB" }],
      fechaEsperada: new Date().toISOString().slice(0, 10),
    });
  };
  const handleCancel = () => {
    setForm(null);
  };
  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setForm((prev) => {
      if (!prev) return prev;
      if (name.startsWith("detalles[0].")) {
        const detalles = prev.detalles ? [...prev.detalles] : [{ ingredienteId: "", cantidad: 1, precioUnitario: 0, moneda: "BOB" }];
        const field = name.replace("detalles[0].", "");
        if (field === "cantidad") {
          detalles[0].cantidad = Number(value);
        } else if (field === "ingredienteId") {
          detalles[0].ingredienteId = value;
        }
        // Mantener los valores por defecto para compatibilidad backend
        detalles[0].precioUnitario = 0;
        detalles[0].moneda = "BOB";
        return { ...prev, detalles };
      }
      if (name === "proveedorId" || name === "fechaEsperada") {
        return { ...prev, [name]: value };
      }
      return prev;
    });
  };
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form) return;
    setSaving(true);
    try {
      await ordenesCompraService.crear(form as CrearOrdenDeCompraCommand);
      await loadOrdenes();
      setForm(null);
    } catch (err) {
      alert("Error al guardar orden de compra");
    } finally {
      setSaving(false);
    }
  };
  const handleAprobar = async (ordenId: string) => {
    try {
      await ordenesCompraService.aprobar(ordenId, "sistema");
      await loadOrdenes();
    } catch (error) {
      // ...
    }
  };

  const handleCancelar = async (ordenId: string) => {
    if (!window.confirm("¿Cancelar esta orden de compra?")) return;
    try {
      await ordenesCompraService.cancelar(ordenId);
      await loadOrdenes();
    } catch (error) {
      alert("Error al cancelar la orden");
    }
  };

  const handleReceiveToggle = (ordenId: string) => {
    if (receiveForId === ordenId) {
      setReceiveForId(null);
      setReceiveData({ codigoLote: "", fechaVencimiento: "", observaciones: "" });
    } else {
      setReceiveForId(ordenId);
      setReceiveData({ codigoLote: "", fechaVencimiento: new Date().toISOString().slice(0, 10), observaciones: "" });
    }
  };

  const handleReceiveChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setReceiveData((prev) => ({ ...prev, [name]: value }));
  };

  const handleRecibir = async (ordenId: string) => {
    if (!receiveData.codigoLote || !receiveData.fechaVencimiento) {
      alert("Código de lote y fecha de vencimiento son requeridos");
      return;
    }
    try {
      await ordenesCompraService.recibir(ordenId, { codigoLote: receiveData.codigoLote, fechaVencimiento: receiveData.fechaVencimiento, observaciones: receiveData.observaciones });
      setReceiveForId(null);
      await loadOrdenes();
    } catch (error) {
      alert("Error al recibir la orden");
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
          <h1>Órdenes de Compra</h1>
          <p>Gestión de órdenes de compra pendientes</p>
        </div>
        <button className="btn btn-primary" onClick={handleAddClick} disabled={!!form}>
          <FiPlus /> Nueva Orden
        </button>
      </div>
      <div className="card">
        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>Proveedor</th>
                <th>Ingrediente</th>
                <th>Cantidad</th>
                <th>Fecha Esperada</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {form && (
                <tr>
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
                    <select
                      name="detalles[0].ingredienteId"
                      value={form.detalles?.[0]?.ingredienteId || ""}
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
                    <input
                      name="detalles[0].cantidad"
                      type="number"
                      value={form.detalles?.[0]?.cantidad ?? 1}
                      onChange={handleChange}
                      min={1}
                      required
                    />
                  </td>
                  <td>
                    <input
                      name="fechaEsperada"
                      type="date"
                      value={form.fechaEsperada || ""}
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
              {displayOrdenes.map((orden) => (
                <tr key={orden.id}>
                  <td>{orden.proveedorNombre}</td>
                  <td>{orden.ingredienteNombre}</td>
                  <td>{orden.cantidad}</td>
                  <td>{orden.fechaEsperada ? format(new Date(orden.fechaEsperada), "yyyy-MM-dd") : ""}</td>
                  <td>
                    {receiveForId === orden.id ? (
                      <div className="action-buttons" style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
                        <input
                          name="codigoLote"
                          value={receiveData.codigoLote}
                          onChange={handleReceiveChange}
                          placeholder="Código Lote"
                          style={{ minWidth: 120 }}
                        />
                        <input
                          name="fechaVencimiento"
                          type="date"
                          value={receiveData.fechaVencimiento}
                          onChange={handleReceiveChange}
                        />
                        <button className="btn btn-primary btn-sm" onClick={() => handleRecibir(orden.id)}>
                          Guardar
                        </button>
                        <button className="btn btn-outline btn-sm" onClick={() => handleReceiveToggle(orden.id)}>
                          Cancelar
                        </button>
                      </div>
                    ) : (
                      <div className="action-buttons" style={{ display: 'flex', gap: 8 }}>
                        <button className="btn btn-primary btn-sm" onClick={() => handleAprobar(orden.id)}>Aprobar</button>
                        <button className="btn btn-outline btn-sm" onClick={() => handleReceiveToggle(orden.id)}>Recibir</button>
                        <button className="btn btn-danger btn-sm" onClick={() => handleCancelar(orden.id)}>Cancelar</button>
                      </div>
                    )}
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

export default OrdenesCompra;
