using Gymeal.Domain.Interfaces.Services;

namespace Gymeal.Infrastructure.Services;

public sealed class ServiceDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
