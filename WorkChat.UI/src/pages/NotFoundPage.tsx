import { Link } from "react-router-dom";
import { Logo } from "../components/ui/Logo";

export function NotFoundPage() {
  return <main className="not-found"><Logo /><strong>404</strong><h1>Página não encontrada</h1><p>O endereço informado não existe ou foi movido.</p><Link className="button button--primary" to="/">Voltar ao início</Link></main>;
}
