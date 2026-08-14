import { ArrowLeft, Clock3, MessageCircleMore } from "lucide-react";
import { useEffect, useState } from "react";
import { Link, Navigate, useParams } from "react-router-dom";
import { ChatRoom } from "../components/chat/ChatRoom";
import { StatusBadge } from "../components/ui/StatusBadge";
import { api } from "../services/api";
import { customerSession } from "../services/customerSession";
import type { Conversa } from "../types/api";

export function ClienteChatPage() {
  const params = useParams();
  const empresaId = params.empresaId ?? "";
  const conversaId = params.conversaId ?? "";
  const session = customerSession.get(empresaId);
  const [conversa, setConversa] = useState<Conversa | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!session) return;
    api.withToken(session.accessToken).get<Conversa>(`/api/conversas/${conversaId}`).then(setConversa).catch((err) => setError(err instanceof Error ? err.message : "Conversa não encontrada."));
  }, [conversaId, session?.accessToken]);

  if (!session) return <Navigate to={`/atendimento/${empresaId}`} replace />;

  return <main className="customer-chat-page">
    <header className="customer-chat-header">
      <Link to={`/atendimento/${empresaId}`} aria-label="Voltar"><ArrowLeft /></Link>
      <span className="customer-chat-avatar"><MessageCircleMore /></span>
      <div><strong>{conversa?.setorNome ?? "Atendimento"}</strong><small>{conversa?.atendenteNome ? `Com ${conversa.atendenteNome}` : "Aguardando atendente"}</small></div>
      {conversa && <StatusBadge status={conversa.status} />}
    </header>
    {error ? <div className="chat-page-error">{error}</div> : conversa ? <>
      {conversa.status === "Waiting" && <div className="queue-notice"><Clock3 size={17} /> Você está na fila{conversa.posicaoFila ? `, posição ${conversa.posicaoFila}` : ""}. Pode enviar sua mensagem enquanto aguarda.</div>}
      <ChatRoom conversaId={conversaId} accessToken={session.accessToken} viewer="Customer" disabled={conversa.status === "Closed"} />
    </> : <div className="chat-page-error">Carregando atendimento...</div>}
  </main>;
}
