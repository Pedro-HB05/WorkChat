import { useEffect, useState } from "react";
import {
  CheckCircle2,
  Inbox,
  RefreshCw,
  Search,
  UserCheck,
  X
} from "lucide-react";
import { useNavigate, useParams } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { ChatRoom } from "../components/chat/ChatRoom";
import { PageHeader } from "../components/layout/PageHeader";
import { StatusBadge } from "../components/ui/StatusBadge";
import { api } from "../services/api";
import { sessionStorageService } from "../services/storage";
import type {
  Conversa,
  PaginaResponse,
  Setor,
  StatusConversa
} from "../types/api";

const filters: Array<{
  label: string;
  value: "" | StatusConversa;
}> = [
    { label: "Todas", value: "" },
    { label: "Aguardando", value: "Waiting" },
    { label: "Em atendimento", value: "Active" },
    { label: "Encerradas", value: "Closed" }
  ];

export function ConversasPage() {
  const { usuario } = useAuth();
  const navigate = useNavigate();
  const { conversaId } = useParams();

  const token =
    sessionStorageService.get()?.accessToken ?? "";

  const [status, setStatus] =
    useState<"" | StatusConversa>("");

  const [search, setSearch] = useState("");

  const [conversas, setConversas] =
    useState<Conversa[]>([]);

  const [selected, setSelected] =
    useState<Conversa | null>(null);

  const [setores, setSetores] =
    useState<Setor[]>([]);

  const [loading, setLoading] =
    useState(true);

  const [error, setError] =
    useState("");

  const [reload, setReload] =
    useState(0);

  useEffect(() => {
    let active = true;

    setLoading(true);
    setError("");

    Promise.all([
      api.get<PaginaResponse<Conversa>>(
        `/api/conversas?tamanho=100${status ? `&status=${status}` : ""
        }`
      ),

      api.get<Setor[]>("/api/setores")
    ])
      .then(([data, departments]) => {
        if (!active) return;

        const nextConversations =
          data.itens ?? [];

        setConversas(nextConversations);

        setSetores(
          departments ?? []
        );

        /*
         * IMPORTANTE:
         *
         * useParams retorna o ID como string.
         *
         * A API pode retornar item.id como número
         * ou como outro tipo.
         *
         * Por isso usamos String() dos dois lados.
         */
        if (conversaId) {
          const found =
            nextConversations.find(
              (item) =>
                String(item.id) ===
                String(conversaId)
            ) ?? null;

          setSelected(found);

          /*
           * Se a conversa não estiver na lista,
           * voltamos para a tela principal.
           */
          if (!found) {
            navigate(
              "/conversas",
              {
                replace: true
              }
            );
          }

          return;
        }

        setSelected(null);
      })
      .catch((err) => {
        if (!active) return;

        console.error(
          "Erro ao carregar conversas:",
          err
        );

        setError(
          err instanceof Error
            ? err.message
            : "Falha ao carregar conversas."
        );
      })
      .finally(() => {
        if (active) {
          setLoading(false);
        }
      });

    return () => {
      active = false;
    };
  }, [
    status,
    reload,
    conversaId,
    navigate
  ]);

  const visible =
    conversas.filter((item) => {
      const texto = `
        ${item.clienteNome ?? ""}
        ${item.clienteEmail ?? ""}
        ${item.setorNome ?? ""}
      `.toLowerCase();

      return texto.includes(
        search.toLowerCase()
      );
    });

  async function action(
    path: string,
    body?: unknown
  ) {
    try {
      setError("");

      await api.post(
        path,
        body
      );

      setReload(
        (value) => value + 1
      );
    } catch (err) {
      console.error(
        "Erro na ação da conversa:",
        err
      );

      setError(
        err instanceof Error
          ? err.message
          : "Não foi possível concluir a ação."
      );
    }
  }

  function abrirConversa(
    conversa: Conversa
  ) {
    setSelected(conversa);

    navigate(
      `/conversas/${conversa.id}`
    );
  }

  function fecharConversaSelecionada() {
    setSelected(null);

    navigate(
      "/conversas",
      {
        replace: false
      }
    );
  }

  return (
    <div className="page page--wide">

      <PageHeader
        eyebrow="ATENDIMENTO"
        title="Central de conversas"
        description="Atenda, transfira e encerre conversas em tempo real."
      />

      <div className="toolbar">

        <div className="tabs">
          {filters.map(
            (filter) => (
              <button
                key={filter.label}
                className={
                  status ===
                    filter.value
                    ? "tab tab--active"
                    : "tab"
                }
                onClick={() =>
                  setStatus(
                    filter.value
                  )
                }
              >
                {filter.label}
              </button>
            )
          )}
        </div>

        <div className="toolbar__actions">

          <label className="search-field">
            <Search size={17} />

            <input
              value={search}
              onChange={(e) =>
                setSearch(
                  e.target.value
                )
              }
              placeholder="Buscar conversa"
            />
          </label>

          <button
            className="icon-button icon-button--border"
            onClick={() =>
              setReload(
                (value) =>
                  value + 1
              )
            }
            aria-label="Atualizar"
          >
            <RefreshCw
              size={18}
            />
          </button>

        </div>

      </div>

      {error && (
        <div className="form-error">
          {error}
        </div>
      )}

      <section
        className={`inbox-layout ${selected
            ? "inbox-layout--selected"
            : ""
          }`}
      >

        <div className="conversation-list">

          {loading ? (
            <div className="state-message">
              Carregando...
            </div>
          ) : visible.length === 0 ? (
            <div className="state-message">

              <Inbox size={30} />

              <strong>
                Fila vazia
              </strong>

              <span>
                Nenhuma conversa encontrada.
              </span>

            </div>
          ) : (
            visible.map(
              (item) => (
                <button
                  key={item.id}
                  className={
                    selected &&
                      String(
                        selected.id
                      ) ===
                      String(
                        item.id
                      )
                      ? "conversation-item conversation-item--active"
                      : "conversation-item"
                  }
                  onClick={() =>
                    abrirConversa(
                      item
                    )
                  }
                >

                  <span className="avatar">
                    {(
                      item.clienteNome ??
                      "C"
                    )
                      .slice(
                        0,
                        2
                      )
                      .toUpperCase()}
                  </span>

                  <span className="conversation-item__copy">

                    <strong>
                      {item.clienteNome ??
                        `Cliente #${item.clienteId}`}
                    </strong>

                    <small>
                      {item.setorNome}
                      {" · "}
                      {item.atendenteNome ??
                        "Não atribuído"}
                    </small>

                  </span>

                  <span>

                    <StatusBadge
                      status={
                        item.status
                      }
                    />

                    <time>
                      {new Intl.DateTimeFormat(
                        "pt-BR",
                        {
                          hour:
                            "2-digit",
                          minute:
                            "2-digit"
                        }
                      ).format(
                        new Date(
                          item.dataAbertura
                        )
                      )}
                    </time>

                  </span>

                </button>
              )
            )
          )}

        </div>

        <div className="agent-chat-panel">

          {!selected ? (

            <div className="state-message">

              <Inbox size={34} />

              <strong>
                Selecione uma conversa
              </strong>

              <span>
                As mensagens e ações
                aparecerão aqui.
              </span>

            </div>

          ) : (

            <>

              <header className="agent-chat-header">

                <button
                  className="icon-button agent-chat-close"
                  onClick={
                    fecharConversaSelecionada
                  }
                  aria-label="Fechar conversa"
                >
                  <X />
                </button>

                <div>

                  <strong>
                    {selected.clienteNome ??
                      `Cliente #${selected.clienteId}`}
                  </strong>

                  <small>
                    {selected.clienteEmail ||
                      "Sem e-mail"}
                    {" · "}
                    {selected.setorNome}
                  </small>

                </div>

                <div className="agent-actions">

                  {selected.status ===
                    "Waiting" && (
                      <button
                        className="button button--soft"
                        onClick={() =>
                          action(
                            `/api/conversas/${selected.id}/assumir`,
                            {
                              atendenteId:
                                usuario?.id
                            }
                          )
                        }
                      >
                        <UserCheck
                          size={16}
                        />

                        Assumir
                      </button>
                    )}

                  {selected.status !==
                    "Closed" &&
                    selected.setorId && (

                      <select
                        aria-label="Transferir setor"
                        value={
                          selected.setorId
                        }
                        onChange={(
                          e
                        ) =>
                          action(
                            `/api/conversas/${selected.id}/transferir`,
                            {
                              setorDestinoId:
                                e
                                  .target
                                  .value,

                              atendenteDestinoId:
                                null,

                              motivo:
                                "Transferência pelo painel"
                            }
                          )
                        }
                      >

                        {setores
                          .filter(
                            (
                              setor
                            ) =>
                              setor.ativo
                          )
                          .map(
                            (
                              setor
                            ) => (
                              <option
                                key={
                                  setor.id
                                }
                                value={
                                  setor.id
                                }
                              >
                                {
                                  setor.nome
                                }
                              </option>
                            )
                          )}

                      </select>

                    )}

                  {selected.status !==
                    "Closed" && (

                      <button
                        className="button button--danger-soft"
                        onClick={() =>
                          action(
                            `/api/conversas/${selected.id}/encerrar`
                          )
                        }
                      >
                        <CheckCircle2
                          size={16}
                        />

                        Encerrar
                      </button>

                    )}

                </div>

              </header>

              <ChatRoom
                key={
                  String(
                    selected.id
                  )
                }
                conversaId={
                  selected.id
                }
                accessToken={
                  token
                }
                viewer="User"
                disabled={
                  selected.status ===
                  "Closed"
                }
              />

            </>

          )}

        </div>

      </section>

    </div>
  );
}