import { apiClient } from "./client";
import type { IUserProfile } from "./auth";

export interface IUpdateProfileRequest {
  fullName?: string;
  age?: number;
  weightKg?: number;
  heightCm?: number;
  bodyFatPct?: number;
  fitnessGoal?: string;
  activityLevel?: string;
  dietaryRestrictions?: string[];
  allergies?: string[];
  dailyCalorieTarget?: number;
  proteinTargetG?: number;
}

export const profileApi = {
  getProfile: () => apiClient.get<IUserProfile>("/api/v1/users/me"),

  updateProfile: (data: IUpdateProfileRequest) =>
    apiClient.put<IUserProfile>("/api/v1/users/me/profile", data),
};
