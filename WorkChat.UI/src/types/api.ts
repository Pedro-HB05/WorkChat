export type Perfil = "Admin" | "Agent" | "Customer";
export type StatusConversa = "Waiting" | "Active" | "Closed";

export interface Usuario {
  id: string;
  empresaId: string;
  nome: string;
  email: string;
  perfil: Perfil;
  ativo: boolean;
  statusAtendimento: string;
  limiteChats: number;
  dataCriacao: string;
}

export interface LoginRequest {
  empresaNome: string;
  email: string;
  senha: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAt: string;
  usuario: Usuario;
}

export interface PaginaResponse<T> {
  itens: T[];
  pagina: number;
  tamanho: number;
  total: number;
  totalPaginas: number;
}

export interface Conversa {
  id: string;
  empresaId: string;
  clienteId: string;
  clienteNome: string | null;
  clienteEmail: string | null;
  setorId: string | null;
  setorNome: string | null;
  atendenteId: string | null;
  atendenteNome: string | null;
  status: StatusConversa;
  prioridade: number;
  posicaoFila: number | null;
  dataAbertura: string;
  dataInicioAtendimento: string | null;
  dataEncerramento: string | null;
}

export interface Cliente {
  id: string;
  empresaId: string;
  nome: string;
  email: string | null;
  telefone: string | null;
  vip: boolean;
  dataCriacao: string;
}

export interface Setor {
  id: string;
  empresaId: string;
  nome: string;
  ativo: boolean;
}

export interface Mensagem {
  id: string;
  conversaId: string;
  remetenteTipo: "User" | "Customer";
  usuarioId: string | null;
  clienteId: string | null;
  conteudo: string;
  dataEnvio: string;
}

export interface Empresa {
  id: string;
  nome: string;
  ativa: boolean;
  mensagemBoasVindas: string;
  mensagemEspera: string;
  mensagemForaHorario: string | null;
  dataCriacao: string;
  limiteChatsPadrao: number;
}

export interface ClienteSession {
  accessToken: string;
  cliente: Cliente;
}
