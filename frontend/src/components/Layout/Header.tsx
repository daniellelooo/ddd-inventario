import React from "react";
import { FiBell, FiSettings, FiUser } from "react-icons/fi";
import "./Header.css";

const Header: React.FC = () => {
  return (
    <header className="header">
      <div className="header-content">
        <div className="header-left">
          <h1>Sistema de Gestión de Inventario</h1>
        </div>
        <div className="header-right">
          <button className="icon-button">
            <FiBell />
          </button>
          <button className="icon-button">
            <FiSettings />
          </button>
          <div className="user-info">
            <FiUser />
            <span>Admin</span>
          </div>
        </div>
      </div>
    </header>
  );
};

export default Header;
