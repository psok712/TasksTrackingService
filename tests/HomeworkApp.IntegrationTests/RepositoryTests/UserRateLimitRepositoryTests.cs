using FluentAssertions;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.IntegrationTests.Fakers;
using HomeworkApp.IntegrationTests.Fixtures;
using Xunit;

namespace HomeworkApp.IntegrationTests.RepositoryTests;

[Collection(nameof(TestFixture))]
public class UserRateLimitRepositoryTests
{
    private readonly IUserRateLimitRepository _repository;

    public UserRateLimitRepositoryTests(TestFixture fixture)
    {
        _repository = fixture.UserRateLimitRepository;
    }

    [Fact]
    public async Task IncRequestPerMinute_Success()
    {
        // Arrange
        var clientIp = ClientIpFaker.Generate().First();
        const long amountIncr = 101;
        
        // Act
        var result = 0L;
        for (var i = 0; i < amountIncr; ++i)
        {
            result = await _repository.IncRequestPerMinute(clientIp, token: default);
        }

        // Asserts
        result.Should().Be(amountIncr);
    }
}