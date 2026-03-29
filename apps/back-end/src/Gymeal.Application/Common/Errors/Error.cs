// NOTE: Error was moved to Gymeal.Domain.Common to allow IRepository interfaces
// to return Result<T> without violating Clean Architecture layer rules.
// This file re-exports the type under the Application namespace for backwards compatibility.
// All new code should use `Gymeal.Domain.Common.Error` directly.
global using Error = Gymeal.Domain.Common.Error;
