import React from "react";
import "./CommonPages.css";

const Movimientos: React.FC = () => {
  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Movimientos de Inventario</h1>
          <p>Historial de movimientos</p>
        </div>
      </div>

      <div className="card">
        <p className="empty-state">Implementar historial de movimientos</p>
      </div>
    </div>
  );
};

export default Movimientos;
