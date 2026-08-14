import { Navigate, Route, Routes } from "react-router-dom";
import { ProtectedRoute } from "../auth/ProtectedRoute";
import { AdminRoute } from "../auth/AdminRoute";
import { useAuth } from "../auth/AuthContext";
import { AppLayout } from "../components/layout/AppLayout";
import { ClientesPage } from "../pages/ClientesPage";
import { ConversasPage } from "../pages/ConversasPage";
import { DashboardPage } from "../pages/DashboardPage";
import { LoginPage } from "../pages/LoginPage";
import { NotFoundPage } from "../pages/NotFoundPage";
import { SetoresPage } from "../pages/SetoresPage";
import { UsuariosPage } from "../pages/UsuariosPage";
import { AtendimentoPublicoPage } from "../pages/AtendimentoPublicoPage";
import { ClienteChatPage } from "../pages/ClienteChatPage";
import { ConfiguracoesEmpresaPage } from "../pages/ConfiguracoesPage";
import { PreferenciasPage } from "../pages/PreferenciasPage";

function ProtectedHomeRedirect() {
  const { isAuthenticated, getDefaultRoute } = useAuth();

  if (!isAuthenticated) return <Navigate to="/login" replace />;
  return <Navigate to={getDefaultRoute()} replace />;
}

export function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/atendimento/:empresaId" element={<AtendimentoPublicoPage />} />
      <Route path="/atendimento/:empresaId/chat/:conversaId" element={<ClienteChatPage />} />
      <Route element={<ProtectedRoute />}>
        <Route element={<AppLayout />}>
          <Route index element={<ProtectedHomeRedirect />} />
          <Route path="dashboard" element={<DashboardPage />} />
          <Route path="conversas" element={<ConversasPage />} />
          <Route path="conversas/:conversaId" element={<ConversasPage />} />
          <Route path="configuracoes" element={<PreferenciasPage />} />
          <Route element={<AdminRoute />}>
            <Route path="clientes" element={<ClientesPage />} />
            <Route path="setores" element={<SetoresPage />} />
            <Route path="usuarios" element={<UsuariosPage />} />
            <Route path="configuracoes-empresa" element={<ConfiguracoesEmpresaPage />} />
          </Route>
        </Route>
      </Route>
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}
