namespace HomeworkApp.Dal.Entities;

public record TaskCommentEntityV1
{
    public long Id { get; init; }
    
    public long TaskId { get; init; }
    
    public long AuthorUserId { get; init; }
    
    public required string Message { get; init; }
}