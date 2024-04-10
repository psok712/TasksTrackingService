using FluentAssertions;
using HomeworkApp.Dal.Models;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.IntegrationTests.Creators;
using HomeworkApp.IntegrationTests.Fakers;
using HomeworkApp.IntegrationTests.Fixtures;
using Xunit;
using TaskStatus = HomeworkApp.Dal.Enums.TaskStatus;

namespace HomeworkApp.IntegrationTests.RepositoryTests;

[Collection(nameof(TestFixture))]
public class TaskRepositoryTests
{
    private readonly ITaskRepository _repository;

    public TaskRepositoryTests(TestFixture fixture)
    {
        _repository = fixture.TaskRepository;
    }

    [Fact]
    public async Task Add_Task_Success()
    {
        // Arrange
        const int count = 5;

        var tasks = TaskEntityV1Faker.Generate(count);
        
        // Act
        var results = await _repository.Add(tasks, default);

        // Asserts
        results.Should().HaveCount(count);
        results.Should().OnlyContain(x => x > 0);
    }
    
    [Fact]
    public async Task Get_SingleTask_Success()
    {
        // Arrange
        var tasks = TaskEntityV1Faker.Generate();
        var taskIds = await _repository.Add(tasks, default);
        var expectedTaskId = taskIds.First();
        var expectedTask = tasks.First()
            .WithId(expectedTaskId);
        
        // Act
        var results = await _repository.Get(new TaskGetModel()
        {
            TaskIds = new[] { expectedTaskId }
        }, default);
        
        // Asserts
        results.Should().HaveCount(1);
        var task = results.Single();

        task.Should().BeEquivalentTo(expectedTask);
    }
    
    [Fact]
    public async Task AssignTask_Success()
    {
        // Arrange
        var assigneeUserId = Create.RandomId();
        
        var tasks = TaskEntityV1Faker.Generate();
        var taskIds = await _repository.Add(tasks, default);
        var expectedTaskId = taskIds.First();
        var expectedTask = tasks.First()
            .WithId(expectedTaskId)
            .WithAssignedToUserId(assigneeUserId);
        var assign = AssignTaskModelFaker.Generate()
            .First()
            .WithTaskId(expectedTaskId)
            .WithAssignToUserId(assigneeUserId);
        
        // Act
        await _repository.Assign(assign, default);
        
        // Asserts
        var results = await _repository.Get(new TaskGetModel()
        {
            TaskIds = new[] { expectedTaskId }
        }, default);
        
        results.Should().HaveCount(1);
        var task = results.Single();
        
        expectedTask = expectedTask with {Status = assign.Status};
        task.Should().BeEquivalentTo(expectedTask);
    }
    
    [Fact]
    public async Task GetSubTasksInStatus_SetHierarchyParentTaskId_ShouldReturnPathToChildTask()
    {
        // Arrange
        var statuses = new[] { TaskStatus.InProgress };
        const int depthHierarchy = 5;
        var hierarchy = await GenerateTaskHierarchyWithStatus(depthHierarchy, status: statuses[0]);
        var parentTaskId = hierarchy.First();
        
        
        // Act
        var results = 
            await _repository.GetSubTasksInStatus(parentTaskId, statuses, token: default);
        
        
        // Asserts
        for (var i = 0; i < results.Length; ++i)
        {
            results[i].ParentTaskIds.Should().BeEquivalentTo(hierarchy.Take(i + 1));
        }
    }
    
    [Fact]
    public async Task GetSubTasksInStatus_SetStatusIsNotExist_ShouldReturnEmpty()
    {
        // Arrange
        var statuses = new[] { TaskStatus.InProgress };
        const int depthHierarchy = 5;
        var hierarchy = await GenerateTaskHierarchyWithStatus(depthHierarchy, status: TaskStatus.Draft);
        var parentTaskId = hierarchy.First();

        
        // Act
        var results = await _repository.GetSubTasksInStatus(parentTaskId, statuses, token: default);
        
        
        // Asserts
        results.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetSubTasksInStatus_SetStatus_ShouldReturnTheSetStatuses()
    {
        // Arrange
        const TaskStatus expectedStatus = TaskStatus.InProgress;
        var statuses = new[] { expectedStatus };
        const int depthHierarchy = 5;
        var hierarchy = await GenerateTaskHierarchyWithStatus(depthHierarchy, status: expectedStatus);
        var parentTaskId = hierarchy.First();

        
        // Act
        var results = await _repository.GetSubTasksInStatus(parentTaskId, statuses, token: default);
        
        
        // Asserts
        results.All(t => t.Status == expectedStatus).Should().BeTrue();
    }
    
    [Fact]
    public async Task GetSubTasksInStatus_SetHierarchyParentTaskId_ShouldReturnWithoutParent()
    {
        // Arrange
        const TaskStatus expectedStatus = TaskStatus.InProgress;
        var statuses = new[] { expectedStatus };
        const int depthHierarchy = 5;
        var hierarchy = await GenerateTaskHierarchyWithStatus(depthHierarchy, status: expectedStatus);
        var parentTaskId = hierarchy.First();

        
        // Act
        var results = await _repository.GetSubTasksInStatus(parentTaskId, statuses, token: default);
        
        
        // Asserts
        results.All(t => t.ParentTaskIds.Any(x => x == t.TaskId)).Should().BeFalse();
    }

    private async Task<long[]> GenerateTaskHierarchyWithStatus(int depthHierarchy, TaskStatus status)
    {
        var tasks = TaskEntityV1Faker.Generate(depthHierarchy)
            .Select(x => x with { Status = (int)status })
            .ToArray();
        var taskIds = await _repository.Add(tasks, default);

        for (var i = 1; i < depthHierarchy; ++i)
        {
            var setParentTaskIdModel = SetParentTaskIdModelFaker.Generate().First()
                .WithTaskId(taskIds[i])
                .WithParentTaskId(taskIds[i - 1]);
            await _repository.SetParentTaskId(setParentTaskIdModel, token: default);
        }

        return taskIds;
    }
}
