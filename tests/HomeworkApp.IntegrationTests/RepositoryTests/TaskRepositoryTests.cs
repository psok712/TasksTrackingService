using System.Net.Http.Headers;
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
    public async Task GetSubTasksInStatus_SetParentTaskAndStatus_ShouldReturnAllChildWithSetStatuses()
    {
        // Arrange
        const TaskStatus expectedStatus = TaskStatus.InProgress;
        var parentTaskId = TaskEntityV1Faker.Generate().First().Id;
        var tasks = TaskEntityV1Faker.Generate(2);
        tasks[0] = tasks.First()
            .WithParentByTaskId(parentTaskId)
            .WithStatusTaskId((int)expectedStatus);
        tasks[1] = tasks.Last()
            .WithParentByTaskId(parentTaskId)
            .WithStatusTaskId((int)TaskStatus.Done);
        
        TaskStatus[] statuses = [expectedStatus];
        await _repository.Add(tasks, default);

        
        // Act
        var results = await _repository.GetSubTasksInStatus(parentTaskId, statuses, CancellationToken.None);
        
        
        // Asserts
        results.All(t => t.ParentTaskIds[0] == parentTaskId).Should().BeTrue();
        results.All(t => t.Status == expectedStatus).Should().BeTrue();
    }
    
    
    [Fact]
    public async Task GetSubTasksInStatus_SetHierarchyParentTaskId_ShouldReturnPathToChildTask()
    {
        // Arrange
        const int expectedParentTaskId = 1;
        const int expectedFirstChildTaskId = 2;
        const int expectedSecondChildTaskId = 3;
        var expectedStatuses = Enum.GetValues(typeof(TaskStatus)).Cast<TaskStatus>().ToArray();
        var tasksParent = TaskEntityV1Faker.Generate(3);
        await _repository.Add(tasksParent, default);
        
        var setParentTaskIdModel = SetParentTaskIdModelFaker.Generate(2);
        setParentTaskIdModel[0] = setParentTaskIdModel.First()
            .WithTaskId(expectedFirstChildTaskId)
            .WithParentTaskId(expectedParentTaskId);
        await _repository.SetParentTaskId(setParentTaskIdModel.First(), CancellationToken.None);
        setParentTaskIdModel[1] = setParentTaskIdModel.Last()
            .WithTaskId(expectedSecondChildTaskId)
            .WithParentTaskId(expectedFirstChildTaskId);
        await _repository.SetParentTaskId(setParentTaskIdModel.Last(), CancellationToken.None);
        
        
        // Act
        var results = 
            await _repository.GetSubTasksInStatus(expectedParentTaskId, expectedStatuses, CancellationToken.None);
        
        
        // Asserts
        results.All(t => t.ParentTaskIds[0] == expectedParentTaskId).Should().BeTrue();
        results.Last(t => t.ParentTaskIds.Last() == expectedFirstChildTaskId).Should();
        results.All(t => t.TaskId != expectedParentTaskId).Should().BeTrue();
    }
}
