using Gymeal.Domain.Interfaces.Services;

namespace Gymeal.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
