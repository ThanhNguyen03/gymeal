// NOTE: Result<T> was moved to Gymeal.Domain.Common to allow IRepository interfaces
// to return Result<T> without violating Clean Architecture layer rules.
// Domain has zero external dependencies — Result<T> is a pure value type.
// All code should use Gymeal.Domain.Common.Result<T> directly.
// This file is intentionally empty — kept to preserve git history and signal the move.
