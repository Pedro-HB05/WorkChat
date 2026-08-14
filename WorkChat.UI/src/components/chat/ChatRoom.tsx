import { useEffect, useRef, useState, type FormEvent } from "react";
import { Send, Wifi, WifiOff } from "lucide-react";
import { api } from "../../services/api";
import { createChatConnection, enterConversation } from "../../services/chat";
import type { Mensagem, PaginaResponse } from "../../types/api";

interface ChatRoomProps {
  conversaId: string;
  accessToken: string;
  viewer: "Customer" | "User";
  disabled?: boolean;
}

export function ChatRoom({ conversaId, accessToken, viewer, disabled }: ChatRoomProps) {
  const [messages, setMessages] = useState<Mensagem[]>([]);
  const [content, setContent] = useState("");
  const [connected, setConnected] = useState(false);
  const [loading, setLoading] = useState(true);
  const [sending, setSending] = useState(false);
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    let active = true;
    let reconnectTimer: ReturnType<typeof setTimeout> | undefined;
    let syncTimer: ReturnType<typeof setTimeout> | undefined;
    const client = api.withToken(accessToken);
    const connection = createChatConnection(accessToken, (message) => {
      if (!active || String(message.conversaId) !== String(conversaId)) return;
      setMessages((current) => current.some((item) => item.id === message.id) ? current : [...current, message]);
    });

    const loadMessages = (initial = false) => client
      .get<PaginaResponse<Mensagem>>(`/api/conversas/${conversaId}/mensagens?tamanho=100`)
      .then((data) => {
        if (!active) return;
        setMessages(data.itens);
      })
      .catch(() => undefined)
      .finally(() => {
        if (!active) return;
        if (initial) setLoading(false);
        syncTimer = setTimeout(() => void loadMessages(), 3000);
      });

    const connect = () => {
      void enterConversation(connection, conversaId)
        .then(() => {
          if (active) setConnected(true);
        })
        .catch(() => {
          if (!active) return;
          setConnected(false);
          reconnectTimer = setTimeout(connect, 3000);
        });
    };

    void loadMessages(true);
    connect();

    connection.onreconnected(() => {
      setConnected(true);
      void connection.invoke("EntrarNaConversa", conversaId);
    });
    connection.onclose(() => setConnected(false));

    return () => {
      active = false;
      clearTimeout(reconnectTimer);
      clearTimeout(syncTimer);
      if (typeof connection.stop === "function") {
        void connection.stop().catch(() => undefined);
      }
    };
  }, [accessToken, conversaId]);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  async function send(event: FormEvent) {
    event.preventDefault();
    const value = content.trim();
    if (!value || sending || disabled) return;
    setSending(true);
    try {
      const message = await api.withToken(accessToken).post<Mensagem>(`/api/conversas/${conversaId}/mensagens`, { conteudo: value });
      setMessages((current) => current.some((item) => item.id === message.id) ? current : [...current, message]);
      setContent("");
    } finally {
      setSending(false);
    }
  }

  return (
    <div className="chat-room">
      <div className={`connection-state ${connected ? "connection-state--online" : ""}`}>
        {connected ? <Wifi size={13} /> : <WifiOff size={13} />}{connected ? "Tempo real conectado" : "Reconectando..."}
      </div>
      <div className="message-list">
        {loading ? <div className="chat-empty">Carregando mensagens...</div> : messages.length === 0 ? <div className="chat-empty"><strong>Conversa iniciada</strong><span>Envie a primeira mensagem para começar.</span></div> : messages.map((message) => {
          const mine = message.remetenteTipo === viewer;
          return <div key={message.id} className={`message ${mine ? "message--mine" : ""}`}><div>{message.conteudo}</div><time>{new Intl.DateTimeFormat("pt-BR", { hour: "2-digit", minute: "2-digit" }).format(new Date(message.dataEnvio))}</time></div>;
        })}
        <div ref={bottomRef} />
      </div>
      <form className="message-composer" onSubmit={send}>
        <textarea value={content} onChange={(event) => setContent(event.target.value)} onKeyDown={(event) => { if (event.key === "Enter" && !event.shiftKey) { event.preventDefault(); event.currentTarget.form?.requestSubmit(); } }} placeholder={disabled ? "Esta conversa foi encerrada" : "Digite sua mensagem..."} disabled={disabled} rows={1} />
        <button className="button button--primary" aria-label="Enviar mensagem" disabled={disabled || sending || !content.trim()}><Send size={18} /></button>
      </form>
    </div>
  );
}
