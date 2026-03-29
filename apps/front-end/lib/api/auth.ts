import { apiClient } from "./client";

export interface IAuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  userId: string;
  email: string;
  role: string;
}

export interface IUserProfile {
  userId: string;
  email: string;
  role: string;
  fullName: string | null;
  avatarUrl: string | null;
  age: number | null;
  weightKg: number | null;
  heightCm: number | null;
  bodyFatPct: number | null;
  fitnessGoal: string;
  activityLevel: string;
  dietaryRestrictions: string[];
  allergies: string[];
  dailyCalorieTarget: number;
  proteinTargetG: number;
}

export const authApi = {
  register: (email: string, password: string, fullName: string) =>
    apiClient.post<IAuthResponse>("/api/v1/auth/register", {
      email,
      password,
      fullName,
    }),

  login: (email: string, password: string) =>
    apiClient.post<IAuthResponse>("/api/v1/auth/login", { email, password }),

  refresh: () => apiClient.post<IAuthResponse>("/api/v1/auth/refresh"),

  logout: () => apiClient.delete<void>("/api/v1/auth/logout"),

  getMe: () => apiClient.get<IUserProfile>("/api/v1/users/me"),
};
