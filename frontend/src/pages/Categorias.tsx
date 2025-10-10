import React from "react";
import { FiPlus } from "react-icons/fi";
import "./CommonPages.css";

const Categorias: React.FC = () => {
  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Categorías</h1>
          <p>Gestión de categorías de ingredientes</p>
        </div>
        <button className="btn btn-primary">
          <FiPlus /> Nueva Categoría
        </button>
      </div>

      <div className="card">
        <p className="empty-state">Implementar gestión de categorías</p>
      </div>
    </div>
  );
};

export default Categorias;
