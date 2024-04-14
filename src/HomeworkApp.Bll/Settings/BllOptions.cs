namespace HomeworkApp.Bll.Settings;

public record BllOptions
{
    public required long RedisRateLimits { get; init; } = long.MaxValue;
}