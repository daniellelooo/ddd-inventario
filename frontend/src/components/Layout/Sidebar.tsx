import React from "react";
import { Link, useLocation } from "react-router-dom";
import {
  FiHome,
  FiPackage,
  FiBox,
  FiUsers,
  FiShoppingCart,
  FiLayers,
} from "react-icons/fi";
import "./Sidebar.css";

const Sidebar: React.FC = () => {
  const location = useLocation();

  const menuItems = [
    { path: "/dashboard", icon: FiHome, label: "Dashboard" },
    { path: "/ingredientes", icon: FiPackage, label: "Ingredientes" },
    { path: "/lotes", icon: FiBox, label: "Lotes" },
    { path: "/proveedores", icon: FiUsers, label: "Proveedores" },
    {
      path: "/ordenes-compra",
      icon: FiShoppingCart,
      label: "Órdenes de Compra",
    },
    { path: "/categorias", icon: FiLayers, label: "Categorías" },
  ];

  return (
    <div className="sidebar">
      <div className="sidebar-header">
        <h2>Inventario DDD</h2>
      </div>
      <nav className="sidebar-nav">
        {menuItems.map((item) => {
          const Icon = item.icon;
          const isActive = location.pathname === item.path;
          return (
            <Link
              key={item.path}
              to={item.path}
              className={`nav-item ${isActive ? "active" : ""}`}
            >
              <Icon className="nav-icon" />
              <span className="nav-label">{item.label}</span>
            </Link>
          );
        })}
      </nav>
    </div>
  );
};

export default Sidebar;
