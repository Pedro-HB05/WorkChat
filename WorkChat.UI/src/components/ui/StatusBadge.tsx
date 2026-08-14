import type { StatusConversa } from "../../types/api";

const labels: Record<StatusConversa, string> = {
  Waiting: "Aguardando",
  Active: "Em atendimento",
  Closed: "Encerrada"
};

export function StatusBadge({ status }: { status: StatusConversa }) {
  return <span className={`status status--${status.toLowerCase()}`}>{labels[status]}</span>;
}
