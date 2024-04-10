using AutoBogus;
using Bogus;
using HomeworkApp.Dal.Models;
using HomeworkApp.IntegrationTests.Creators;

namespace HomeworkApp.IntegrationTests.Fakers;

public static class SetParentTaskIdModelFaker
{
    private static readonly object Lock = new();

    private static readonly Faker<SetParentTaskIdModel> Faker = new AutoFaker<SetParentTaskIdModel>()
        .RuleFor(x => x.TaskId, _ => Create.RandomId())
        .RuleFor(x => x.ParentTaskId, _ => Create.RandomId());

    public static SetParentTaskIdModel[] Generate(int count = 1)
    {
        lock (Lock)
        {
            return Faker.Generate(count).ToArray();
        }
    }

    public static SetParentTaskIdModel WithTaskId(
        this SetParentTaskIdModel src, 
        long taskId)
        => src with { TaskId = taskId };
    
    public static SetParentTaskIdModel WithParentTaskId(
        this SetParentTaskIdModel src, 
        long parentTaskId)
        => src with { ParentTaskId = parentTaskId };    
}