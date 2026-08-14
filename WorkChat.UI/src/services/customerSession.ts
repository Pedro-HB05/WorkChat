import type { ClienteSession } from "../types/api";

const key = (empresaId: string) => `workchat.customer.${empresaId}`;

export const customerSession = {
  get(empresaId: string): ClienteSession | null {
    try {
      const value = localStorage.getItem(key(empresaId));
      return value ? JSON.parse(value) as ClienteSession : null;
    } catch {
      return null;
    }
  },
  set(empresaId: string, session: ClienteSession) {
    localStorage.setItem(key(empresaId), JSON.stringify(session));
  },
  clear(empresaId: string) {
    localStorage.removeItem(key(empresaId));
  }
};
