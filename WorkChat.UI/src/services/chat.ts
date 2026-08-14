import { HubConnectionBuilder, HubConnectionState, LogLevel } from "@microsoft/signalr";
import { API_BASE_URL } from "./api";
import type { Mensagem } from "../types/api";

export function createChatConnection(accessToken: string, onMessage: (message: Mensagem) => void) {
  const connection = new HubConnectionBuilder()
    .withUrl(`${API_BASE_URL}/hubs/chat`, { accessTokenFactory: () => accessToken })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();
  connection.on("MensagemRecebida", onMessage);
  return connection;
}

export async function enterConversation(connection: ReturnType<typeof createChatConnection>, conversaId: string) {
  if (connection.state === HubConnectionState.Disconnected) await connection.start();
  await connection.invoke("EntrarNaConversa", conversaId);
}
