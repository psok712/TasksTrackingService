using Grpc.Core;
using Grpc.Core.Interceptors;
using HomeworkApp.Bll.Services.Interfaces;

namespace HomeworkApp.Interceptors;

public class RateLimiterInterceptor : Interceptor
{
    private IRateLimiterService _service;

    public RateLimiterInterceptor(IRateLimiterService service)
    {
        _service = service;
    }
    
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var clientIp = context.RequestHeaders.FirstOrDefault(x => x.Key == "x-r256-user-ip")?.Value;

        try
        {
            if (clientIp != null)
                await _service.ThrowIfTooManyRequest(clientIp, token: default);

            return await continuation(request, context);
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}