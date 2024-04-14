using HomeworkApp.Utils.Providers.Interfaces;

namespace HomeworkApp.Utils.Providers;

public class DateTimeOffsetProvider : IDateTimeOffsetProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}