import { MessageCircleMore } from "lucide-react";

export function Logo({ compact = false }: { compact?: boolean }) {
  return (
    <div className="logo" aria-label="WorkChat">
      <span className="logo__mark"><MessageCircleMore size={22} /></span>
      {!compact && <span>Work<span>Chat</span></span>}
    </div>
  );
}
