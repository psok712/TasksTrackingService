namespace HomeworkApp.Utils.Providers.Interfaces;

public interface IDateTimeOffsetProvider
{
    DateTimeOffset UtcNow { get; }
}