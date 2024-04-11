using System.Text.Json;
using FluentAssertions;
using HomeworkApp.Bll.Services.Interfaces;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.IntegrationTests.Creators;
using HomeworkApp.IntegrationTests.Fakers;
using HomeworkApp.IntegrationTests.Fixtures;
using Microsoft.Extensions.Caching.Distributed;
using Xunit;

namespace HomeworkApp.IntegrationTests.ServiceTests;

[Collection(nameof(TestFixture))]
public class TaskServiceTests
{
    private readonly ITaskCommentRepository _repository;

    private readonly IDistributedCache _cache;

    private readonly ITaskService _service;

    public TaskServiceTests(TestFixture fixture)
    {
        _repository = fixture.TaskCommentRepository;
        _cache = fixture.DistributedCache;
        _service = fixture.TaskService;
    }
    
    [Fact]
    public async Task GetComments_AddTaskCommentsToDatabase_ShouldReturnCommentFromDatabase()
    {
        // Arrange
        const int amountTaskComment = 6;
        var taskCommentsId = Create.RandomId();
        var taskComments = TaskCommentEntityV1Faker.Generate(amountTaskComment);
        var addTasks = taskComments
            .Select(async t => await _repository.Add(t.WithTaskId(taskCommentsId), token: default))
            .ToArray();
        await Task.WhenAll(addTasks);
        
        // Act
        var result = await _service.GetComments(taskCommentsId, token: default);

        
        // Asserts
        result.Should().HaveCount(amountTaskComment);
        result.All(t => taskComments.Any(c => t.Comment == c.Message)).Should().BeTrue();
    }
    
    [Fact]
    public async Task GetComments_AddTaskCommentsToCache_ShouldReturnCommentFromCache()
    {
        // Arrange
        const int amountTaskComment = 5;
        var taskCommentsId = Create.RandomId();
        var cacheKey = $"comment_tasks:{taskCommentsId}";
        var taskMessages = TaskMessageFaker.Generate(amountTaskComment)
            .Select(t => t.WithTaskId(taskCommentsId))
            .ToArray();
        var taskJson = JsonSerializer.Serialize(taskMessages);
        await _cache.SetStringAsync(
            cacheKey, 
            taskJson,
            new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
            },
            token: default);
        
        
        // Act
        var result = await _service.GetComments(taskCommentsId, token: default);
        
        
        // Asserts
        result.Should().HaveCount(amountTaskComment);
        result.All(t => taskMessages.Any(c => t.Comment == c.Comment)).Should().BeTrue();
    }
    
    [Fact]
    public async Task GetComments_WaitDeletedFromCacheAddedTaskComments_ShouldReturnEmpty()
    {
        // Arrange
        const int amountTaskComment = 5;
        var taskCommentsId = Create.RandomId();
        var cacheKey = $"comment_tasks:{taskCommentsId}";
        var taskMessages = TaskMessageFaker.Generate(amountTaskComment)
            .Select(t => t.WithTaskId(taskCommentsId))
            .ToArray();
        var taskJson = JsonSerializer.Serialize(taskMessages);
        await _cache.SetStringAsync(
            cacheKey, 
            taskJson,
            new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
            },
            token: default);
        
        
        // Act
        await Task.Delay(5000);
        var result = await _service.GetComments(taskCommentsId, token: default);
        
        
        // Asserts
        result.Should().BeEmpty();
    }
}