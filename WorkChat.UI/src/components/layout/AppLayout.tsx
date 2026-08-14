import { useState } from "react";
import { BarChart3, Building2, LogOut, Menu, MessageSquareText, Settings, Users, UsersRound, X } from "lucide-react";
import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";
import { Logo } from "../ui/Logo";
import { api } from "../../services/api";

const navigation = [
  { to: "/dashboard", label: "Visão geral", icon: BarChart3 },
  { to: "/conversas", label: "Conversas", icon: MessageSquareText },
  { to: "/clientes", label: "Clientes", icon: Users, adminOnly: true },
  { to: "/setores", label: "Setores", icon: Building2, adminOnly: true },
  { to: "/usuarios", label: "Equipe", icon: UsersRound, adminOnly: true },
  { to: "/configuracoes-empresa", label: "Configurações da empresa", icon: Building2, adminOnly: true },
  { to: "/configuracoes", label: "Configurações", icon: Settings }
];

export function AppLayout() {
  const { usuario, logout } = useAuth();
  const [menuOpen, setMenuOpen] = useState(false);
  const [presence, setPresence] = useState(usuario?.statusAtendimento ?? "OFFLINE");
  const initials = usuario?.nome.split(" ").slice(0, 2).map((part) => part[0]).join("").toUpperCase();

  return (
    <div className="app-shell">
      {menuOpen && <button className="sidebar-backdrop" onClick={() => setMenuOpen(false)} aria-label="Fechar menu" />}
      <aside className={`sidebar ${menuOpen ? "sidebar--open" : ""}`}>
        <div className="sidebar__header">
          <Logo />
          <button className="icon-button sidebar__close" onClick={() => setMenuOpen(false)} aria-label="Fechar menu"><X /></button>
        </div>
        <nav className="sidebar__nav" aria-label="Navegação principal">
          <span className="sidebar__label">ESPAÇO DE TRABALHO</span>
          {navigation.filter((item) => !item.adminOnly || usuario?.perfil === "Admin").map(({ to, label, icon: Icon }) => (
            <NavLink key={to} to={to} onClick={() => setMenuOpen(false)} className={({ isActive }) => isActive ? "nav-link nav-link--active" : "nav-link"}>
              <Icon size={19} /> {label}
            </NavLink>
          ))}
        </nav>
        <div className="sidebar__profile">
          <span className="avatar">{initials}</span>
          <span className="profile-copy"><strong>{usuario?.nome}</strong><small>{usuario?.perfil.toLowerCase()}</small></span>
          <button className="icon-button" onClick={logout} title="Sair" aria-label="Sair"><LogOut size={18} /></button>
        </div>
        {usuario?.perfil === "Agent" && <select className="presence-select" value={presence} onChange={async (event) => { const status = event.target.value; setPresence(status); await api.patch(`/api/usuarios/${usuario.id}/status`, { status }); }}><option value="Online">Online</option><option value="Busy">Ocupado</option><option value="Pause">Pausa</option><option value="Away">Ausente</option><option value="Offline">Offline</option></select>}
      </aside>
      <main className="main-content">
        <header className="mobile-header">
          <button className="icon-button" onClick={() => setMenuOpen(true)} aria-label="Abrir menu"><Menu /></button>
          <Logo />
        </header>
        <Outlet />
      </main>
    </div>
  );
}
