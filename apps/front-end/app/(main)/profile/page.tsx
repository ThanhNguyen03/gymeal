"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import { useAuth } from "@/lib/auth/AuthProvider";
import { profileApi, type IUpdateProfileRequest } from "@/lib/api/profile";
import { ApiError } from "@/lib/api/client";

// ── Step definitions ─────────────────────────────────────────────────────────

const FITNESS_GOALS = [
  { value: "Cut", label: "Cut", description: "Lose fat while preserving muscle" },
  { value: "Bulk", label: "Bulk", description: "Build muscle mass" },
  { value: "Maintain", label: "Maintain", description: "Stay at current weight" },
  { value: "Endurance", label: "Endurance", description: "Improve cardio performance" },
  { value: "Recomp", label: "Recomp", description: "Lose fat and gain muscle simultaneously" },
] as const;

const ACTIVITY_LEVELS = [
  { value: "Sedentary", label: "Sedentary", description: "Little or no exercise" },
  { value: "LightlyActive", label: "Lightly Active", description: "Light exercise 1–3 days/week" },
  { value: "ModeratelyActive", label: "Moderately Active", description: "Moderate exercise 3–5 days/week" },
  { value: "VeryActive", label: "Very Active", description: "Hard exercise 6–7 days/week" },
  { value: "ExtremelyActive", label: "Extremely Active", description: "Very hard exercise + physical job" },
] as const;

const DIETARY_RESTRICTIONS = [
  "Vegan", "Vegetarian", "Halal", "Kosher", "Gluten-Free", "Dairy-Free",
  "Low-Carb", "Keto", "Paleo", "Whole30",
];

const ALLERGIES = [
  "Peanuts", "Tree Nuts", "Shellfish", "Fish", "Dairy", "Eggs",
  "Soy", "Wheat", "Sesame",
];

const TOTAL_STEPS = 6;

// ── Component ─────────────────────────────────────────────────────────────────

export default function ProfilePage() {
  const router = useRouter();
  const { refreshUser } = useAuth();

  const [step, setStep] = useState(0);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Form state
  const [fitnessGoal, setFitnessGoal] = useState("");
  const [age, setAge] = useState("");
  const [weightKg, setWeightKg] = useState("");
  const [heightCm, setHeightCm] = useState("");
  const [bodyFatPct, setBodyFatPct] = useState("");
  const [activityLevel, setActivityLevel] = useState("");
  const [dietaryRestrictions, setDietaryRestrictions] = useState<string[]>([]);
  const [allergies, setAllergies] = useState<string[]>([]);
  const [dailyCalorieTarget, setDailyCalorieTarget] = useState("");
  const [proteinTargetG, setProteinTargetG] = useState("");

  function toggleItem(
    list: string[],
    setList: (v: string[]) => void,
    item: string,
  ) {
    setList(
      list.includes(item) ? list.filter((x) => x !== item) : [...list, item],
    );
  }

  function canAdvance(): boolean {
    if (step === 0) return fitnessGoal !== "";
    if (step === 2) return activityLevel !== "";
    return true;
  }

  async function handleSave() {
    setError(null);
    setIsLoading(true);

    const data: IUpdateProfileRequest = {
      fitnessGoal: fitnessGoal || undefined,
      activityLevel: activityLevel || undefined,
      age: age ? Number(age) : undefined,
      weightKg: weightKg ? Number(weightKg) : undefined,
      heightCm: heightCm ? Number(heightCm) : undefined,
      bodyFatPct: bodyFatPct ? Number(bodyFatPct) : undefined,
      dietaryRestrictions: dietaryRestrictions.length ? dietaryRestrictions : undefined,
      allergies: allergies.length ? allergies : undefined,
      dailyCalorieTarget: dailyCalorieTarget ? Number(dailyCalorieTarget) : undefined,
      proteinTargetG: proteinTargetG ? Number(proteinTargetG) : undefined,
    };

    try {
      await profileApi.updateProfile(data);
      await refreshUser();
      router.push("/dashboard");
    } catch (err) {
      if (err instanceof ApiError) {
        setError("Failed to save profile. Please try again.");
      } else {
        setError("Something went wrong. Please try again.");
      }
    } finally {
      setIsLoading(false);
    }
  }

  // ── Step renderers ──────────────────────────────────────────────────────────

  function renderStep() {
    switch (step) {
      case 0:
        return (
          <div className="space-y-3">
            <p className="text-sm text-neutral-500 mb-4">
              This helps us calculate your daily calorie and macro targets.
            </p>
            {FITNESS_GOALS.map((goal) => (
              <button
                key={goal.value}
                type="button"
                onClick={() => setFitnessGoal(goal.value)}
                className={`w-full text-left px-4 py-3 rounded-lg border-2 transition-colors ${
                  fitnessGoal === goal.value
                    ? "border-primary-500 bg-primary-50"
                    : "border-neutral-200 hover:border-neutral-300"
                }`}
              >
                <span className="font-medium text-neutral-900">{goal.label}</span>
                <span className="block text-xs text-neutral-500 mt-0.5">
                  {goal.description}
                </span>
              </button>
            ))}
          </div>
        );

      case 1:
        return (
          <div className="space-y-4">
            <p className="text-sm text-neutral-500 mb-4">
              Used for accurate macro calculations. All fields are optional.
            </p>
            {[
              { id: "age", label: "Age", value: age, setter: setAge, placeholder: "e.g. 28", unit: "years" },
              { id: "weight", label: "Weight", value: weightKg, setter: setWeightKg, placeholder: "e.g. 75", unit: "kg" },
              { id: "height", label: "Height", value: heightCm, setter: setHeightCm, placeholder: "e.g. 178", unit: "cm" },
              { id: "bodyfat", label: "Body Fat %", value: bodyFatPct, setter: setBodyFatPct, placeholder: "e.g. 18", unit: "%" },
            ].map(({ id, label, value, setter, placeholder, unit }) => (
              <div key={id}>
                <label
                  htmlFor={id}
                  className="block text-sm font-medium text-neutral-700 mb-1"
                >
                  {label}
                </label>
                <div className="relative">
                  <input
                    id={id}
                    type="number"
                    min="0"
                    value={value}
                    onChange={(e) => setter(e.target.value)}
                    placeholder={placeholder}
                    className="w-full px-3 py-2 pr-10 border border-neutral-300 rounded-md text-sm
                               focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                  />
                  <span className="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-neutral-400">
                    {unit}
                  </span>
                </div>
              </div>
            ))}
          </div>
        );

      case 2:
        return (
          <div className="space-y-3">
            <p className="text-sm text-neutral-500 mb-4">
              Affects your total daily energy expenditure (TDEE).
            </p>
            {ACTIVITY_LEVELS.map((level) => (
              <button
                key={level.value}
                type="button"
                onClick={() => setActivityLevel(level.value)}
                className={`w-full text-left px-4 py-3 rounded-lg border-2 transition-colors ${
                  activityLevel === level.value
                    ? "border-primary-500 bg-primary-50"
                    : "border-neutral-200 hover:border-neutral-300"
                }`}
              >
                <span className="font-medium text-neutral-900">{level.label}</span>
                <span className="block text-xs text-neutral-500 mt-0.5">
                  {level.description}
                </span>
              </button>
            ))}
          </div>
        );

      case 3:
        return (
          <div>
            <p className="text-sm text-neutral-500 mb-4">Select all that apply.</p>
            <div className="flex flex-wrap gap-2">
              {DIETARY_RESTRICTIONS.map((item) => (
                <button
                  key={item}
                  type="button"
                  onClick={() =>
                    toggleItem(dietaryRestrictions, setDietaryRestrictions, item)
                  }
                  className={`px-3 py-1.5 rounded-full text-sm border transition-colors ${
                    dietaryRestrictions.includes(item)
                      ? "border-primary-500 bg-primary-50 text-primary-700"
                      : "border-neutral-300 text-neutral-700 hover:border-neutral-400"
                  }`}
                >
                  {item}
                </button>
              ))}
            </div>
          </div>
        );

      case 4:
        return (
          <div>
            <p className="text-sm text-neutral-500 mb-4">
              Select all that apply. We&apos;ll never suggest meals containing these.
            </p>
            <div className="flex flex-wrap gap-2">
              {ALLERGIES.map((item) => (
                <button
                  key={item}
                  type="button"
                  onClick={() => toggleItem(allergies, setAllergies, item)}
                  className={`px-3 py-1.5 rounded-full text-sm border transition-colors ${
                    allergies.includes(item)
                      ? "border-danger bg-danger-light text-danger"
                      : "border-neutral-300 text-neutral-700 hover:border-neutral-400"
                  }`}
                >
                  {item}
                </button>
              ))}
            </div>
          </div>
        );

      case 5:
        return (
          <div className="space-y-4">
            <p className="text-sm text-neutral-500 mb-4">
              Leave blank to use auto-calculated targets based on your goal and stats.
            </p>
            {[
              {
                id: "calories",
                label: "Daily Calorie Target",
                value: dailyCalorieTarget,
                setter: setDailyCalorieTarget,
                placeholder: "e.g. 2400",
                unit: "kcal",
              },
              {
                id: "protein",
                label: "Protein Target",
                value: proteinTargetG,
                setter: setProteinTargetG,
                placeholder: "e.g. 160",
                unit: "g",
              },
            ].map(({ id, label, value, setter, placeholder, unit }) => (
              <div key={id}>
                <label
                  htmlFor={id}
                  className="block text-sm font-medium text-neutral-700 mb-1"
                >
                  {label}
                </label>
                <div className="relative">
                  <input
                    id={id}
                    type="number"
                    min="0"
                    value={value}
                    onChange={(e) => setter(e.target.value)}
                    placeholder={placeholder}
                    className="w-full px-3 py-2 pr-14 border border-neutral-300 rounded-md text-sm
                               focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                  />
                  <span className="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-neutral-400">
                    {unit}
                  </span>
                </div>
              </div>
            ))}
          </div>
        );

      default:
        return null;
    }
  }

  const STEP_TITLES = [
    "Fitness goal",
    "Body stats",
    "Activity level",
    "Dietary restrictions",
    "Allergies",
    "Macro targets",
  ];

  return (
    <div className="max-w-lg mx-auto">
      {/* Progress bar */}
      <div className="mb-8">
        <div className="flex justify-between text-xs text-neutral-500 mb-2">
          <span>
            Step {step + 1} of {TOTAL_STEPS}
          </span>
          <span>{Math.round(((step + 1) / TOTAL_STEPS) * 100)}%</span>
        </div>
        <div className="h-1.5 bg-neutral-200 rounded-full overflow-hidden">
          <div
            className="h-full bg-primary-500 rounded-full transition-all duration-300"
            style={{ width: `${((step + 1) / TOTAL_STEPS) * 100}%` }}
          />
        </div>
      </div>

      {/* Card */}
      <div className="bg-surface-base rounded-card shadow-card p-6">
        <h2 className="text-lg font-semibold text-neutral-900 mb-1 capitalize">
          {STEP_TITLES[step]}
        </h2>

        {error && (
          <div
            role="alert"
            className="mb-4 p-3 bg-danger-light text-danger rounded-md text-sm"
          >
            {error}
          </div>
        )}

        <div className="mt-4">{renderStep()}</div>

        {/* Navigation */}
        <div className="mt-6 flex gap-3">
          {step > 0 && (
            <button
              type="button"
              onClick={() => setStep((s) => s - 1)}
              disabled={isLoading}
              className="flex-1 py-2 px-4 border border-neutral-300 rounded-md text-sm font-medium
                         text-neutral-700 hover:bg-neutral-50 transition-colors disabled:opacity-50"
            >
              Back
            </button>
          )}

          {step < TOTAL_STEPS - 1 ? (
            <button
              type="button"
              onClick={() => setStep((s) => s + 1)}
              disabled={!canAdvance()}
              className="flex-1 py-2 px-4 bg-primary-500 hover:bg-primary-600 disabled:bg-neutral-300
                         text-white font-medium rounded-md text-sm transition-colors
                         focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2"
            >
              Next
            </button>
          ) : (
            <button
              type="button"
              onClick={handleSave}
              disabled={isLoading}
              className="flex-1 py-2 px-4 bg-primary-500 hover:bg-primary-600 disabled:bg-neutral-300
                         text-white font-medium rounded-md text-sm transition-colors
                         focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2"
            >
              {isLoading ? "Saving…" : "Save & continue"}
            </button>
          )}
        </div>

        {/* Skip link (steps 1, 3, 4, 5 are optional) */}
        {[1, 3, 4, 5].includes(step) && step < TOTAL_STEPS - 1 && (
          <p className="mt-3 text-center">
            <button
              type="button"
              onClick={() => setStep((s) => s + 1)}
              className="text-sm text-neutral-400 hover:text-neutral-600"
            >
              Skip for now
            </button>
          </p>
        )}
        {step === TOTAL_STEPS - 1 && (
          <p className="mt-3 text-center">
            <button
              type="button"
              onClick={() => router.push("/dashboard")}
              className="text-sm text-neutral-400 hover:text-neutral-600"
            >
              Skip for now
            </button>
          </p>
        )}
      </div>
    </div>
  );
}
