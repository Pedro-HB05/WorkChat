import { KeyRound, Moon, Save, Sun } from "lucide-react";
import { useState, type FormEvent } from "react";
import { PageHeader } from "../components/layout/PageHeader";
import { api } from "../services/api";
import { applyTheme, getTheme, type Theme } from "../services/theme";

export function PreferenciasPage() {
  const [theme, setTheme] = useState<Theme>(getTheme);
  const [message, setMessage] = useState("");
  const [saving, setSaving] = useState(false);

  function changeTheme(nextTheme: Theme) {
    setTheme(nextTheme);
    applyTheme(nextTheme);
  }

  async function changePassword(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = event.currentTarget;
    const data = new FormData(form);
    const newPassword = String(data.get("novaSenha") ?? "");
    const confirmation = String(data.get("confirmacaoSenha") ?? "");

    if (newPassword !== confirmation) {
      setMessage("A confirmação da nova senha não confere.");
      return;
    }

    setSaving(true);
    setMessage("");
    try {
      await api.post("/api/usuarios/me/alterar-senha", {
        senhaAtual: data.get("senhaAtual"),
        novaSenha: newPassword
      });
      form.reset();
      setMessage("Senha alterada com sucesso.");
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "Não foi possível alterar a senha.");
    } finally {
      setSaving(false);
    }
  }

  return <div className="page">
    <PageHeader eyebrow="PREFERÊNCIAS" title="Configurações" description="Gerencie sua senha e a aparência do sistema." />
    <div className="preferences-grid">
      <section className="settings-panel entity-form">
        <div className="settings-section-title"><Sun size={20} /><div><h2>Aparência</h2><p>Escolha o tema usado neste navegador.</p></div></div>
        <div className="theme-options">
          <button type="button" className={theme === "light" ? "theme-option theme-option--active" : "theme-option"} onClick={() => changeTheme("light")}><Sun size={20} /><strong>Claro</strong></button>
          <button type="button" className={theme === "dark" ? "theme-option theme-option--active" : "theme-option"} onClick={() => changeTheme("dark")}><Moon size={20} /><strong>Escuro</strong></button>
        </div>
      </section>
      <form className="settings-panel entity-form" onSubmit={changePassword}>
        <div className="settings-section-title"><KeyRound size={20} /><div><h2>Alterar senha</h2><p>Use pelo menos 8 caracteres.</p></div></div>
        <label>Senha atual<input name="senhaAtual" type="password" required autoComplete="current-password" /></label>
        <div className="form-grid"><label>Nova senha<input name="novaSenha" type="password" minLength={8} maxLength={100} required autoComplete="new-password" /></label><label>Confirmar nova senha<input name="confirmacaoSenha" type="password" minLength={8} maxLength={100} required autoComplete="new-password" /></label></div>
        {message && <div className="service-notice">{message}</div>}
        <button className="button button--primary" disabled={saving}><Save size={17} /> {saving ? "Salvando..." : "Alterar senha"}</button>
      </form>
    </div>
  </div>;
}
