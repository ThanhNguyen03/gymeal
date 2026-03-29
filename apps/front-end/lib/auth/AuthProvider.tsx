"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
} from "react";
import { ApiError } from "@/lib/api/client";
import { authApi, type IUserProfile } from "@/lib/api/auth";

interface IAuthContextValue {
  user: IUserProfile | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (
    email: string,
    password: string,
    fullName: string,
  ) => Promise<void>;
  logout: () => Promise<void>;
  refreshUser: () => Promise<void>;
}

const AuthContext = createContext<IAuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<IUserProfile | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const refreshUser = useCallback(async () => {
    try {
      const profile = await authApi.getMe();
      setUser(profile);
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) {
        setUser(null);
      }
    }
  }, []);

  // On mount: check if there's an existing valid session
  useEffect(() => {
    refreshUser().finally(() => setIsLoading(false));
  }, [refreshUser]);

  const login = useCallback(async (email: string, password: string) => {
    await authApi.login(email, password);
    const profile = await authApi.getMe();
    setUser(profile);
  }, []);

  const register = useCallback(
    async (email: string, password: string, fullName: string) => {
      await authApi.register(email, password, fullName);
      const profile = await authApi.getMe();
      setUser(profile);
    },
    [],
  );

  const logout = useCallback(async () => {
    await authApi.logout().catch(() => {
      // WARNING: Swallow logout errors intentionally — clear local state regardless.
      // Reason: Even if the server-side revocation fails, the user expects to be logged out.
      // The access token will naturally expire in 15 minutes.
    });
    setUser(null);
  }, []);

  return (
    <AuthContext.Provider
      value={{
        user,
        isLoading,
        isAuthenticated: user !== null,
        login,
        register,
        logout,
        refreshUser,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): IAuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return ctx;
}
