using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.Dal.Settings;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace HomeworkApp.Dal.Repositories;

public class UserRateLimitRepository : RedisRepository, IUserRateLimitRepository
{
    protected override TimeSpan KeyTtl => TimeSpan.FromMinutes(1);
    
    protected override string KeyPrefix => "rate-limit";

    public UserRateLimitRepository(IOptions<DalOptions> dalSettings) : base(dalSettings.Value) { }
    
    public async Task<long> IncRequestPerMinute(string clientIp, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var connection = await GetConnection();
        
        var key = GetKey(clientIp);

        if (!connection.KeyExists(key))
        {
            connection.StringSet(key, value: 0, KeyTtl, When.NotExists);
        }

        return connection.StringIncrement(key);
    }
}