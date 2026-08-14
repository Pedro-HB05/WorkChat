import type { LucideIcon } from "lucide-react";

export function ResourcePlaceholder({ icon: Icon, title, description }: { icon: LucideIcon; title: string; description: string }) {
  return <section className="resource-placeholder"><span><Icon size={28} /></span><h2>{title}</h2><p>{description}</p></section>;
}
