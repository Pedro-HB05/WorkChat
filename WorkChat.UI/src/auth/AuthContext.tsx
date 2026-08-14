import { createContext, useCallback, useContext, useMemo, useState, type PropsWithChildren } from "react";
import { api } from "../services/api";
import { sessionStorageService } from "../services/storage";
import type { LoginRequest, LoginResponse, Usuario } from "../types/api";

interface AuthContextValue {
  usuario: Usuario | null;
  isAuthenticated: boolean;
  isAdmin: boolean;
  isAgent: boolean;
  login: (credentials: LoginRequest) => Promise<LoginResponse>;
  getDefaultRoute: () => string;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: PropsWithChildren) {
  const [session, setSession] = useState<LoginResponse | null>(() => sessionStorageService.get());

  const getDefaultRoute = useCallback(() => {
    if (session?.usuario?.perfil === "Admin") return "/dashboard";
    return "/conversas";
  }, [session]);

  const login = useCallback(async (credentials: LoginRequest) => {
    const response = await api.post<LoginResponse>("/api/auth/login", credentials);
    sessionStorageService.set(response);
    setSession(response);
    return response;
  }, []);

  const logout = useCallback(() => {
    sessionStorageService.clear();
    setSession(null);
  }, []);

  const value = useMemo(() => ({
    usuario: session?.usuario ?? null,
    isAuthenticated: Boolean(session),
    isAdmin: session?.usuario?.perfil === "Admin",
    isAgent: session?.usuario?.perfil === "Agent",
    login,
    getDefaultRoute,
    logout
  }), [session, login, getDefaultRoute, logout]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth deve ser usado dentro de AuthProvider.");
  return context;
}
