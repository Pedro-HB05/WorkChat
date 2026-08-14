import type { LoginResponse } from "../types/api";

const SESSION_KEY = "workchat.session";

export const sessionStorageService = {
  get(): LoginResponse | null {
    const raw = localStorage.getItem(SESSION_KEY);
    if (!raw) return null;

    try {
      const session = JSON.parse(raw) as LoginResponse;
      if (new Date(session.expiresAt).getTime() <= Date.now()) {
        localStorage.removeItem(SESSION_KEY);
        return null;
      }
      return session;
    } catch {
      localStorage.removeItem(SESSION_KEY);
      return null;
    }
  },
  set(session: LoginResponse) {
    localStorage.setItem(SESSION_KEY, JSON.stringify(session));
  },
  clear() {
    localStorage.removeItem(SESSION_KEY);
  }
};
