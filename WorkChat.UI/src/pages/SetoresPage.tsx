import { Building2, Edit3, Plus } from "lucide-react";
import { useEffect, useState, type FormEvent } from "react";
import { PageHeader } from "../components/layout/PageHeader";
import { Modal } from "../components/ui/Modal";
import { api } from "../services/api";
import type { Setor } from "../types/api";

export function SetoresPage() {
  const [items, setItems] = useState<Setor[]>([]); const [editing, setEditing] = useState<Setor | "new" | null>(null); const [reload, setReload] = useState(0); const [error, setError] = useState("");
  useEffect(() => { api.get<Setor[]>("/api/setores").then(setItems).catch((e) => setError(e.message)); }, [reload]);
  async function save(event: FormEvent<HTMLFormElement>) { event.preventDefault(); const d = new FormData(event.currentTarget); try { if (editing === "new") await api.post("/api/setores", { nome: d.get("nome") }); else if (editing) await api.put(`/api/setores/${editing.id}`, { nome: d.get("nome"), ativo: d.get("ativo") === "on" }); setEditing(null); setReload((x) => x + 1); } catch (e) { setError(e instanceof Error ? e.message : "Falha ao salvar."); } }
  return <div className="page"><PageHeader eyebrow="CONFIGURAÇÃO" title="Setores" description="Organize filas e áreas responsáveis pelo atendimento." actions={<button className="button button--primary" onClick={() => setEditing("new")}><Plus size={18} /> Novo setor</button>} />{error && <div className="form-error">{error}</div>}<section className="resource-grid">{items.map((x) => <article className="resource-card" key={x.id}><span className="resource-card__icon"><Building2 /></span><div><h2>{x.nome}</h2><span className={x.ativo ? "active-label" : "inactive-label"}>{x.ativo ? "Ativo" : "Inativo"}</span></div><button className="icon-button icon-button--border" onClick={() => setEditing(x)}><Edit3 size={16} /></button></article>)}</section>{editing && <Modal title={editing === "new" ? "Novo setor" : "Editar setor"} onClose={() => setEditing(null)}><form className="entity-form" onSubmit={save}><label>Nome do setor<input name="nome" required defaultValue={editing === "new" ? "" : editing.nome} /></label>{editing !== "new" && <label className="checkbox"><input name="ativo" type="checkbox" defaultChecked={editing.ativo} /> Setor ativo</label>}<div className="modal-actions"><button type="button" className="button button--ghost" onClick={() => setEditing(null)}>Cancelar</button><button className="button button--primary">Salvar</button></div></form></Modal>}</div>;
}
