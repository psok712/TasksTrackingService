using HomeworkApp.Bll.Services.Interfaces;
using HomeworkApp.Bll.Settings;
using HomeworkApp.Dal.Repositories.Interfaces;
using Microsoft.Extensions.Options;

namespace HomeworkApp.Bll.Services;

public class RateLimiterService : IRateLimiterService
{
    private readonly IUserRateLimitRepository _userRateLimitRepository;
    
    private readonly BllOptions _bllSettings;

    public RateLimiterService(IUserRateLimitRepository userRateLimitRepository, IOptions<BllOptions> bllSettings)
    {
        _userRateLimitRepository = userRateLimitRepository;
        _bllSettings = bllSettings.Value;
    }


    public async Task ThrowIfTooManyRequest(string clientIp, CancellationToken token)
    {
        var requestPerMinute = _bllSettings.RedisRateLimits;

        var actualScore = await _userRateLimitRepository.IncRequestPerMinute(clientIp, token);

        if (actualScore <= requestPerMinute)
            return;
        
        throw new InvalidOperationException("Too many requests");
    }
}