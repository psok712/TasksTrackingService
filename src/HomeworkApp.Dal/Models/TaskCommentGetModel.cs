namespace HomeworkApp.Dal.Models;

public record TaskCommentGetModel
{
    public long TaskId { get; init; }
    
    public bool CanceledTasks { get; init; } = false;
}