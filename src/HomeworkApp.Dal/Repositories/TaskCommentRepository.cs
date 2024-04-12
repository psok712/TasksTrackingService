using Dapper;
using HomeworkApp.Dal.Entities;
using HomeworkApp.Dal.Models;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.Dal.Settings;
using HomeworkApp.Utils.Providers.Interfaces;
using Microsoft.Extensions.Options;

namespace HomeworkApp.Dal.Repositories;

public class TaskCommentRepository(
    IOptions<DalOptions> dalSettings,
    IDateTimeOffsetProvider dateTimeOffsetProvider)
    : PgRepository(dalSettings.Value), ITaskCommentRepository
{
    public async Task<long> Add(TaskCommentEntityV1 comment, CancellationToken token)
    {
        const string sqlQuery = @"
insert into task_comments (task_id, author_user_id, message, at) 
select task_id, author_user_id, message, at
  from UNNEST(@Comment)
returning id;
";
        await using var connection = await GetConnection();
        var ids = await connection.QueryAsync<long>(
            new CommandDefinition(
                sqlQuery,
                new
                {
                    Comment = new[] { comment }
                },
                cancellationToken: token));

        return ids.First();
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
        @params.Add($"ModifiedAt", model.ModifiedAt!.Value);

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
        @params.Add("DeletedAt", dateTimeOffsetProvider.UtcNow);

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
     , at
     , modified_at
     , deleted_at
  from task_comments
";

        var conditions = new List<string>();
        var @params = new DynamicParameters();
        @params.Add("TaskId", model.TaskId);

        if (!model.IncludeDeleted)
        {
            conditions.Add($"deleted_at is null");
        }
        conditions.Add("task_id = @TaskId");
        
        var cmd = new CommandDefinition(
            baseSql + $" where {string.Join(" and ", conditions)} order by at desc",
            @params,
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: token);
        
        await using var connection = await GetConnection();
        return (await connection.QueryAsync<TaskCommentEntityV1>(cmd))
            .ToArray();
    }
}