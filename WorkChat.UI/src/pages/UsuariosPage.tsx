import { Edit3, Plus } from "lucide-react";
import { useEffect, useState, type FormEvent } from "react";
import { useAuth } from "../auth/AuthContext";
import { PageHeader } from "../components/layout/PageHeader";
import { Modal } from "../components/ui/Modal";
import { api } from "../services/api";
import type { PaginaResponse, Setor, Usuario } from "../types/api";

export function UsuariosPage() {
  const { usuario: atual } = useAuth();
  const [usuarios, setUsuarios] = useState<Usuario[]>([]);
  const [setores, setSetores] = useState<Setor[]>([]);
  const [edicao, setEdicao] = useState<Usuario | "novo" | null>(null);
  const [recarregar, setRecarregar] = useState(0);
  const [erro, setErro] = useState("");

  useEffect(() => {
    Promise.all([api.get<PaginaResponse<Usuario>>("/api/usuarios?tamanho=100"), api.get<Setor[]>("/api/setores")])
      .then(([u, s]) => { setUsuarios(u.itens); setSetores(s); })
      .catch((e) => setErro(e.message));
  }, [recarregar]);

  async function salvar(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const dados = new FormData(event.currentTarget);
    try {
      let id: string;
      if (edicao === "novo") {
        const criado = await api.post<Usuario>("/api/usuarios", { empresaId: atual?.empresaId, nome: dados.get("nome"), email: dados.get("email"), senha: dados.get("senha"), perfil: dados.get("perfil"), limiteChats: Number(dados.get("limiteChats")) });
        id = criado.id;
      } else if (edicao) {
        id = edicao.id;
        await api.put(`/api/usuarios/${id}`, { nome: dados.get("nome"), email: dados.get("email"), perfil: dados.get("perfil"), ativo: dados.get("ativo") === "on", limiteChats: Number(dados.get("limiteChats")) });
      } else return;
      const setorId = String(dados.get("setorId") ?? "");
      if (setorId) await api.post(`/api/usuarios/${id}/setores`, { setorId });
      setEdicao(null); setRecarregar((x) => x + 1);
    } catch (e) { setErro(e instanceof Error ? e.message : "Falha ao salvar usuário."); }
  }

  return <div className="page"><PageHeader eyebrow="ADMINISTRAÇÃO" title="Equipe" description="Gerencie acessos, perfis, presença e capacidade." actions={<button className="button button--primary" onClick={() => setEdicao("novo")}><Plus size={18} /> Novo usuário</button>} />{erro && <div className="form-error">{erro}</div>}<section className="table-panel"><div className="table-scroll"><table><thead><tr><th>Usuário</th><th>Perfil</th><th>Presença</th><th>Limite</th><th>Status</th><th /></tr></thead><tbody>{usuarios.map((x) => <tr key={x.id}><td><strong>{x.nome}</strong><small>{x.email}</small></td><td>{x.perfil}</td><td>{x.statusAtendimento}</td><td>{x.limiteChats} chats</td><td>{x.ativo ? "Ativo" : "Inativo"}</td><td><button className="icon-button icon-button--border" onClick={() => setEdicao(x)}><Edit3 size={16} /></button></td></tr>)}</tbody></table></div></section>
    {edicao && <Modal title={edicao === "novo" ? "Novo usuário" : "Editar usuário"} onClose={() => setEdicao(null)}><form className="entity-form" onSubmit={salvar}><div className="form-grid"><label>Nome<input name="nome" required defaultValue={edicao === "novo" ? "" : edicao.nome} /></label><label>E-mail<input name="email" type="email" required defaultValue={edicao === "novo" ? "" : edicao.email} /></label>{edicao === "novo" && <label>Senha<input name="senha" type="password" minLength={8} required /></label>}<label>Perfil<select name="perfil" defaultValue={edicao === "novo" ? "Agent" : edicao.perfil}><option value="Agent">Atendente</option><option value="Admin">Administrador</option></select></label><label>Limite<input name="limiteChats" type="number" min={1} defaultValue={edicao === "novo" ? 5 : edicao.limiteChats} /></label><label>Setor<select name="setorId"><option value="">Nenhum</option>{setores.map((x) => <option value={x.id} key={x.id}>{x.nome}</option>)}</select></label></div>{edicao !== "novo" && <label className="checkbox"><input name="ativo" type="checkbox" defaultChecked={edicao.ativo} /> Usuário ativo</label>}<div className="modal-actions"><button type="button" className="button button--ghost" onClick={() => setEdicao(null)}>Cancelar</button><button className="button button--primary">Salvar</button></div></form></Modal>}
  </div>;
}
