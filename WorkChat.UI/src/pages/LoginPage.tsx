import { useState, type FormEvent } from "react";
import { ArrowRight, Eye, EyeOff, Headphones, ShieldCheck, Sparkles } from "lucide-react";
import { Navigate, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { Logo } from "../components/ui/Logo";

export function LoginPage() {
  const { login, isAuthenticated, getDefaultRoute } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  if (isAuthenticated) return <Navigate to={getDefaultRoute()} replace />;

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    setLoading(true);
    setError("");
    try {
      const response = await login({
        empresaNome: String(data.get("empresaNome")),
        email: String(data.get("email")),
        senha: String(data.get("senha"))
      });

      const from = (location.state as { from?: string } | null)?.from;
      const destination = from && from !== "/login" ? from : response.usuario.perfil === "Admin" ? "/dashboard" : "/conversas";
      navigate(destination, { replace: true });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Não foi possível entrar.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="login-page">
      <section className="login-brand">
        <Logo />
        <div className="login-brand__content">
          <span className="hero-pill"><Sparkles size={15} /> Atendimento mais humano</span>
          <h1>Converse melhor.<br /><span>Resolva mais rápido.</span></h1>
          <p>Uma central de atendimento simples para aproximar sua equipe de cada cliente.</p>
          <div className="login-features">
            <span><Headphones /> Atendimento organizado</span>
            <span><ShieldCheck /> Dados protegidos</span>
          </div>
        </div>
        <small>© 2026 WorkChat. Todos os direitos reservados.</small>
      </section>
      <section className="login-panel">
        <form className="login-card" onSubmit={handleSubmit}>
          <div className="login-card__mobile-logo"><Logo /></div>
          <span className="eyebrow">BEM-VINDO DE VOLTA</span>
          <h2>Acesse sua conta</h2>
          <p>Informe os dados do seu espaço de trabalho.</p>
          <label>Empresa<input name="empresaNome" placeholder="Nome da empresa" autoComplete="organization" required /></label>
          <label>E-mail<input name="email" type="email" placeholder="voce@empresa.com" autoComplete="email" required /></label>
          <label>Senha
            <span className="password-field">
              <input name="senha" type={showPassword ? "text" : "password"} placeholder="Sua senha" autoComplete="current-password" required />
              <button type="button" onClick={() => setShowPassword((value) => !value)} aria-label={showPassword ? "Ocultar senha" : "Exibir senha"}>
                {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </span>
          </label>
          {error && <div className="form-error" role="alert">{error}</div>}
          <button className="button button--primary button--full" disabled={loading}>
            {loading ? "Entrando..." : <>Entrar <ArrowRight size={18} /></>}
          </button>
        </form>
      </section>
    </main>
  );
}
