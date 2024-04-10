namespace HomeworkApp.Dal.Models;

public record SetParentTaskIdModel
{
    public required long TaskId { get; init; }
    
    public required long ParentTaskId { get; init; }
}