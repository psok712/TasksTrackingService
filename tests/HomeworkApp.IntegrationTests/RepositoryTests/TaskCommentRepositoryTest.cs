using FluentAssertions;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.IntegrationTests.Fakers;
using HomeworkApp.IntegrationTests.Fixtures;
using HomeworkApp.Utils.Providers.Interfaces;
using Moq;
using Xunit;

namespace HomeworkApp.IntegrationTests.RepositoryTests;

[Collection(nameof(TestFixture))]
public class TaskCommentRepositoryTest
{
    private readonly ITaskCommentRepository _repository;
    private readonly Mock<IDateTimeOffsetProvider> _dateTimeOffsetProviderFake; 
    
    public TaskCommentRepositoryTest(TestFixture fixture)
    {
        _repository = fixture.TaskCommentRepository;
        _dateTimeOffsetProviderFake = fixture.DateTimeOffsetProviderFaker;
    }
    
    [Fact]
    public async Task Get_AddTaskComment_ShouldReturnThisTaskComment()
    {
        // Arrange
        var taskComment = TaskCommentEntityV1Faker.Generate().First();
        var expectedTaskId = taskComment.TaskId;
        await _repository.Add(taskComment, token: default);
        var commentGetModel = TaskCommentGetModelFaker.Generate().First()
            .WithTaskId(taskComment.TaskId)
            .WithIncludeDeleted(false);
        
        
        // Act
        var result = await _repository.Get(commentGetModel, default);
        
        
        // Asserts
        result.Should().NotBeNull();
        result.All(t => t.TaskId == expectedTaskId && t.DeletedAt is null).Should().BeTrue();
    }
    
    [Fact]
    public async Task Add_TaskComment_ShouldReturnPositiveId()
    {
        // Arrange
        var taskComment = TaskCommentEntityV1Faker.Generate().First();
        
        
        // Act
        var results = await _repository.Add(taskComment, default);

        
        // Asserts
        var expectedTaskComment = taskComment.WithId(results);
        var commentGetModel = TaskCommentGetModelFaker.Generate().First()
            .WithTaskId(taskComment.TaskId)
            .WithIncludeDeleted(false);
        var taskAfterAdd = await _repository.Get(commentGetModel, token: default);
        taskAfterAdd.First().Should().BeEquivalentTo(expectedTaskComment);
        results.Should().BePositive();
    }
    
    
    [Fact]
    public async Task Update_TaskComment_ShouldUpdateCommentAndModifiedIsNotNull()
    {
        // Arrange
        const string updateMessage = "update message";
        var modifiedTime = DateTimeOffset.UtcNow;
        var taskComment = TaskCommentEntityV1Faker.Generate().First();
        var taskCommentId = await _repository.Add(taskComment, default);
        var commentTaskUpdate = taskComment
            .WithId(taskCommentId)
            .WithMessage(updateMessage)
            .WithModifiedAt(modifiedTime);
        
        
        // Act
        await _repository.Update(commentTaskUpdate, default);

        
        // Asserts
        var commentGetModel = TaskCommentGetModelFaker.Generate().First()
            .WithTaskId(taskComment.TaskId)
            .WithIncludeDeleted(true);
        var taskAfterUpdate = await _repository.Get(commentGetModel, token: default);
        taskAfterUpdate.First().Should().BeEquivalentTo(commentTaskUpdate);
    }
    
    [Fact]
    public async Task SetDeleted_TaskCommentIncludeDeleted_Success()
    {
        // Arrange
        var deletedTime = DateTimeOffset.UtcNow;
        _dateTimeOffsetProviderFake
            .Setup(x => x.UtcNow)
            .Returns(deletedTime);
        var taskComment = TaskCommentEntityV1Faker.Generate().First();
        var taskCommentId = await _repository.Add(taskComment, default);
        var expectedTaskAfterDelete = taskComment
            .WithId(taskCommentId)
            .WithDeletedAt(deletedTime);
        
        
        // Act
        await _repository.SetDeleted(taskCommentId, default);

        
        // Asserts
        var commentGetModel = TaskCommentGetModelFaker.Generate().First()
            .WithTaskId(taskComment.TaskId)
            .WithIncludeDeleted(true);
        var taskAfterDelete = await _repository.Get(commentGetModel, token: default);
        taskAfterDelete.First().Should().BeEquivalentTo(expectedTaskAfterDelete);
    }
    
    [Fact]
    public async Task SetDeleted_TaskComment_ShouldReturnEmpty()
    {
        // Arrange
        var deletedTime = DateTimeOffset.UtcNow;
        _dateTimeOffsetProviderFake
            .Setup(x => x.UtcNow)
            .Returns(deletedTime);
        var taskComment = TaskCommentEntityV1Faker.Generate().First();
        var taskCommentId = await _repository.Add(taskComment, default);
        var expectedTaskAfterDelete = taskComment
            .WithId(taskCommentId)
            .WithDeletedAt(deletedTime);
        
        
        // Act
        await _repository.SetDeleted(taskCommentId, default);

        
        // Asserts
        var commentGetModel = TaskCommentGetModelFaker.Generate().First()
            .WithTaskId(taskComment.TaskId)
            .WithIncludeDeleted(true);
        var taskAfterDelete = await _repository.Get(commentGetModel, token: default);
        taskAfterDelete.First().Should().BeEquivalentTo(expectedTaskAfterDelete);
    }
}