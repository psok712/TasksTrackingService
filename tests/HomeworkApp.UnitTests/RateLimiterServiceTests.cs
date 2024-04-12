using FluentAssertions;
using HomeworkApp.Bll.Services;
using HomeworkApp.Bll.Services.Interfaces;
using HomeworkApp.Bll.Settings;
using HomeworkApp.Dal.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using Moq;

namespace HomeworkApp.UnitTests;

public class RateLimiterServiceTests
{
    private readonly IRateLimiterService _service;
    
    private const long RateLimits = 100;
    
    private readonly Mock<IUserRateLimitRepository> _productRepositoryFake = new(MockBehavior.Strict);
    
    public RateLimiterServiceTests()
    {
        Mock<IOptions<BllOptions>> bllOptions = new();
        var defaultOptions = new BllOptions { RedisRateLimits = RateLimits };
        bllOptions
            .Setup(f => f.Value)
            .Returns(defaultOptions);
        
        _service = new RateLimiterService(_productRepositoryFake.Object, bllOptions.Object);
    }
    
    [Fact]
    public async Task ThrowIfTooManyRequest_ClientSentFewRequest_Success()
    {
        // Arrange
        const string clientIp = "144.217.253.149";
        const long amountClientRequest = 10;
        _productRepositoryFake
            .Setup(f => f.IncRequestPerMinute(clientIp, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(amountClientRequest));

        // Act
        try
        {
            await _service.ThrowIfTooManyRequest(clientIp, token: default);
        }
        catch (InvalidOperationException)
        {
            // Asserts
            Assert.Fail("ThrowIfTooManyRequest threw an exception InvalidOperationException.");
        }
    }
    
    [Fact]
    public async Task ThrowIfTooManyRequest_ClientSentTooManyRequest_Failed()
    {
        // Arrange
        const string clientIp = "144.217.253.149";
        const long amountClientRequest = 101;
        _productRepositoryFake
            .Setup(f => f.IncRequestPerMinute(clientIp, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(amountClientRequest));

        // Act
        try
        {
            await _service.ThrowIfTooManyRequest(clientIp, token: default);
            
            // Asserts
            Assert.Fail("ThrowIfTooManyRequest did not throw an exception InvalidOperationException.");
        }
        catch (InvalidOperationException e)
        {
            e.Message.Should().Be("Too many requests");
        }
        
    }
}