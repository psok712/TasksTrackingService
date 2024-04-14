namespace HomeworkApp.Bll.Services.Interfaces;

public interface IRateLimiterService
{
    Task ThrowIfTooManyRequest(string clientIp, CancellationToken token);
}