namespace HomeworkApp.Dal.Repositories.Interfaces;

public interface IUserRateLimitRepository : IRedisRepository
{
    Task<long> IncRequestPerMinute(string clientIp, CancellationToken token);
}