namespace HomeworkApp.Bll.Models;

public class TaskMessageModel
{
    public required long TaskId { get; init; }
    public required string Comment { get; init; }
    public required bool IsDeleted { get; init; }
    public required DateTimeOffset At { get; init; }
}