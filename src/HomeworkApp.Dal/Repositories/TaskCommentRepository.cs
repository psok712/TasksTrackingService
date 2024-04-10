using Dapper;
using HomeworkApp.Dal.Entities;
using HomeworkApp.Dal.Models;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.Dal.Settings;
using HomeworkApp.Utils.Provider;
using Microsoft.Extensions.Options;

namespace HomeworkApp.Dal.Repositories;

public class TaskCommentRepository : PgRepository, ITaskCommentRepository
{
    private readonly IDateTimeProvider _dateTimeProvider;
    public TaskCommentRepository(
        IOptions<DalOptions> dalSettings,
        IDateTimeProvider dateTimeProvider) : base(dalSettings.Value)
    {
        _dateTimeProvider = dateTimeProvider;
    }
    
    public async Task<long> Add(TaskCommentEntityV1 comment, CancellationToken token)
    {
        const string sqlQuery = @"
insert into task_comments (task_id, author_user_id, message, at) 
select task_id, author_user_id, message, at
  from UNNEST(@Comment)
returning id;
";
        await using var connection = await GetConnection();
        var ids = await connection.ExecuteAsync(
            new CommandDefinition(
                sqlQuery,
                new
                {
                    Comment = new[] { comment }
                },
                cancellationToken: token));
        
        return ids;
    }

    public async Task Update(TaskCommentEntityV1 model, CancellationToken token)
    {
        const string sqlQuery = @"
update task_comments
set message = @Message, modified_at = @ModifiedAt
where id = @Id;
";
        
        var @params = new DynamicParameters();
        @params.Add("Id", model.Id);
        @params.Add($"Message", model.Message);
        @params.Add($"ModifiedAt", model.ModifiedAt!.Value.ToUniversalTime());

        await using var connection = await GetConnection();
        await connection.ExecuteAsync(sqlQuery, @params);
    }

    public async Task SetDeleted(long taskCommentId, CancellationToken token)
    {
        const string sqlQuery = @"
update task_comments
set deleted_at = @DeletedAt
where id = @Id;
";
        
        var @params = new DynamicParameters();
        @params.Add("Id", taskCommentId);
        @params.Add("DeletedAt", _dateTimeProvider.UtcNow.ToUniversalTime());

        await using var connection = await GetConnection();
        await connection.ExecuteAsync(sqlQuery, @params);
    }

    public async Task<TaskCommentEntityV1[]> Get(TaskCommentGetModel model, CancellationToken token)
    {
        var baseSql = @"
select id
     , task_id
     , message
     , author_user_id
     , modified_at
     , deleted_at
  from task_comments
";

        var conditions = new List<string>();

        if (!model.IncludeDeleted)
        {
            conditions.Add($" where deleted_at is null");
        }
        
        conditions.Add(" order by id desc");
        
        var cmd = new CommandDefinition(
            baseSql + string.Join("\n", conditions),
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: token);
        
        await using var connection = await GetConnection();
        return (await connection.QueryAsync<TaskCommentEntityV1>(cmd))
            .ToArray();
    }
}