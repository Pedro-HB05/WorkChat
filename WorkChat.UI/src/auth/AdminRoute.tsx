import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "./AuthContext";

export function AdminRoute() {
  const { isAdmin, getDefaultRoute } = useAuth();

  return isAdmin
    ? <Outlet />
    : <Navigate to={getDefaultRoute()} replace />;
}
