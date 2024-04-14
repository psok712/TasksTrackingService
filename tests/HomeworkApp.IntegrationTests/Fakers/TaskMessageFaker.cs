using AutoBogus;
using Bogus;
using HomeworkApp.Bll.Models;
using HomeworkApp.IntegrationTests.Creators;

namespace HomeworkApp.IntegrationTests.Fakers;

public static class TaskMessageFaker
{
    private static readonly object Lock = new();

    private static readonly Faker<TaskMessage> Faker = new AutoFaker<TaskMessage>()
        .RuleFor(x => x.TaskId, _ => Create.RandomId())
        .RuleFor(x => x.IsDeleted, f => f.Random.Bool())
        .RuleFor(x => x.At, _ => DateTimeOffset.UtcNow)
        .RuleFor(x => x.Comment, t => t.Random.Word())
        .RuleForType(typeof(long), f => f.Random.Long(0L));

    public static TaskMessage[] Generate(int count = 1)
    {
        lock (Lock)
        {
            return Faker.Generate(count).ToArray();
        }
    }
    
    public static TaskMessage WithTaskId(
            this TaskMessage src, 
            long taskId)
            => src with { TaskId = taskId };
}