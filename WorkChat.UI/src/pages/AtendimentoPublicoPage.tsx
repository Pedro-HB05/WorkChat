import { useEffect, useState, type FormEvent } from "react";
import { ArrowRight, Building2, Check, Headphones, MessageCircleMore } from "lucide-react";
import { useNavigate, useParams } from "react-router-dom";
import { Logo } from "../components/ui/Logo";
import { api } from "../services/api";
import { customerSession } from "../services/customerSession";
import type { ClienteSession, Conversa, Setor } from "../types/api";

export function AtendimentoPublicoPage() {
  const { empresaId: rawEmpresaId } = useParams();
  const empresaId = rawEmpresaId ?? "";
  const navigate = useNavigate();
  const [session, setSession] = useState<ClienteSession | null>(() => customerSession.get(empresaId));
  const [setores, setSetores] = useState<Setor[]>([]);
  const [setorId, setSetorId] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!session) return;
    api.withToken(session.accessToken).get<Setor[]>("/api/setores")
      .then((items) => { setSetores(items.filter((item) => item.ativo)); setSetorId(items.find((item) => item.ativo)?.id ?? null); })
      .catch((err) => setError(err instanceof Error ? err.message : "Não foi possível carregar os setores."));
  }, [session]);

  async function identify(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!empresaId) { setError("Link de atendimento inválido."); return; }
    const data = new FormData(event.currentTarget);
    setLoading(true); setError("");
    try {
      const result = await api.post<ClienteSession>("/api/clientes", { empresaId, nome: data.get("nome"), email: data.get("email") || null, telefone: data.get("telefone") || null });
      customerSession.set(empresaId, result);
      setSession(result);
    } catch (err) { setError(err instanceof Error ? err.message : "Não foi possível iniciar o atendimento."); }
    finally { setLoading(false); }
  }

  async function startChat() {
    if (!session || !setorId) return;
    setLoading(true); setError("");
    try {
      const conversa = await api.withToken(session.accessToken).post<Conversa>("/api/conversas", { setorId, prioridade: 0 });
      navigate(`/atendimento/${empresaId}/chat/${conversa.id}`);
    } catch (err) { setError(err instanceof Error ? err.message : "Não foi possível abrir a conversa."); }
    finally { setLoading(false); }
  }

  return (
    <main className="public-service">
      <header><Logo /><span><span className="presence-dot" /> Atendimento online</span></header>
      <section className="service-card">
        <div className="service-card__icon"><MessageCircleMore size={29} /></div>
        {!session ? <>
          <span className="eyebrow">FALE COM A GENTE</span><h1>Como podemos ajudar?</h1><p>Preencha seus dados para iniciar um atendimento. Não é necessário criar senha.</p>
          <form onSubmit={identify} className="service-form">
            <label>Seu nome<input name="nome" placeholder="Nome completo" required maxLength={100} /></label>
            <label>E-mail <small>(opcional)</small><input name="email" type="email" placeholder="voce@email.com" /></label>
            <label>Telefone <small>(opcional)</small><input name="telefone" placeholder="(00) 00000-0000" /></label>
            {error && <div className="form-error">{error}</div>}
            <button className="button button--primary button--full" disabled={loading}>{loading ? "Aguarde..." : <>Continuar <ArrowRight size={18} /></>}</button>
          </form>
        </> : <>
          <span className="eyebrow">OLÁ, {session.cliente.nome.split(" ")[0].toUpperCase()}</span><h1>Escolha um setor</h1><p>Selecione o assunto para direcionarmos você à equipe certa.</p>
          <div className="department-list">
            {setores.map((setor) => <button key={setor.id} className={setorId === setor.id ? "department department--selected" : "department"} onClick={() => setSetorId(setor.id)}><Building2 size={19} /><span>{setor.nome}</span>{setorId === setor.id && <Check size={17} />}</button>)}
          </div>
          {setores.length === 0 && !error && <div className="service-notice">Nenhum setor disponível no momento.</div>}
          {error && <div className="form-error">{error}</div>}
          <button className="button button--primary button--full" onClick={startChat} disabled={!setorId || loading}>{loading ? "Abrindo conversa..." : <><Headphones size={18} /> Iniciar atendimento</>}</button>
        </>}
      </section>
      <footer>Atendimento seguro por <strong>WorkChat</strong></footer>
    </main>
  );
}
