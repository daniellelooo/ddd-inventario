import React from "react";
import {
  BrowserRouter as Router,
  Routes,
  Route,
  Navigate,
} from "react-router-dom";
import Layout from "./components/Layout/Layout";
import Dashboard from "./pages/Dashboard";
import Ingredientes from "./pages/Ingredientes";
import Lotes from "./pages/Lotes";
import Proveedores from "./pages/Proveedores";
import OrdenesCompra from "./pages/OrdenesCompra";
import Categorias from "./pages/Categorias";

function App() {
  return (
    <Router>
      <Layout>
        <Routes>
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route path="/dashboard" element={<Dashboard />} />
          <Route path="/ingredientes" element={<Ingredientes />} />
          <Route path="/lotes" element={<Lotes />} />
          <Route path="/proveedores" element={<Proveedores />} />
          <Route path="/ordenes-compra" element={<OrdenesCompra />} />
          <Route path="/categorias" element={<Categorias />} />
        </Routes>
      </Layout>
    </Router>
  );
}

export default App;
