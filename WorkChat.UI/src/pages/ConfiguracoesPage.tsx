import { Save } from "lucide-react";
import { useEffect, useState, type FormEvent } from "react";
import { PageHeader } from "../components/layout/PageHeader";
import { api } from "../services/api";
import type { Empresa } from "../types/api";

export function ConfiguracoesEmpresaPage() {
  const [empresa, setEmpresa] = useState<Empresa | null>(null);
  const [mensagem, setMensagem] = useState("");
  useEffect(() => { api.get<Empresa>("/api/empresas").then(setEmpresa).catch((e) => setMensagem(e.message)); }, []);
  async function salvar(event: FormEvent<HTMLFormElement>) { event.preventDefault(); if (!empresa) return; const d = new FormData(event.currentTarget); try { await api.put("/api/empresas", { nome: d.get("nome"), ativa: d.get("ativa") === "on", mensagemBoasVindas: d.get("mensagemBoasVindas"), mensagemEspera: d.get("mensagemEspera"), mensagemForaHorario: d.get("mensagemForaHorario") || null }); setMensagem("Configurações salvas."); } catch (e) { setMensagem(e instanceof Error ? e.message : "Falha ao salvar."); } }
  return <div className="page"><PageHeader eyebrow="ADMINISTRAÇÃO" title="Configurações da empresa" description="Personalize o atendimento da empresa." />{empresa ? <form className="settings-panel entity-form" onSubmit={salvar}><div className="form-grid"><label>Nome<input name="nome" defaultValue={empresa.nome} required /></label><label>ID público<input value={empresa.id} disabled /></label></div><label>Boas-vindas<textarea name="mensagemBoasVindas" rows={3} defaultValue={empresa.mensagemBoasVindas ?? ""} required /></label><label>Espera<textarea name="mensagemEspera" rows={3} defaultValue={empresa.mensagemEspera ?? ""} required /></label><label>Fora do horário<textarea name="mensagemForaHorario" rows={3} defaultValue={empresa.mensagemForaHorario ?? ""} /></label><label className="checkbox"><input name="ativa" type="checkbox" defaultChecked={empresa.ativa} /> Empresa ativa</label>{mensagem && <div className="service-notice">{mensagem}</div>}<button className="button button--primary"><Save size={17} /> Salvar</button><div className="public-link"><strong>Link público</strong><code>{window.location.origin}/atendimento/{empresa.id}</code></div></form> : <div className="state-message">{mensagem || "Carregando..."}</div>}</div>;
}
