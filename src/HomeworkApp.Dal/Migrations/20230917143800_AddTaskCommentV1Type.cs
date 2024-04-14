using FluentMigrator;

namespace Route256.Week5.Workshop.PriceCalculator.Dal.Migrations;

[Migration(20230917143800, TransactionBehavior.None)]
public class AddTaskCommentV1Type : Migration{
    public override void Up()
    {
        const string sql = @"
DO $$
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'comment_v1') THEN
            CREATE TYPE comment_v1 as
            (
                  task_id             bigint
                , author_user_id      bigint
                , message             text
                , at                  timestamp with time zone
            );
        END IF;
    END
$$;";
        
        Execute.Sql(sql);
    }

    public override void Down()
    {
        const string sql = @"
DO $$
    BEGIN
        DROP TYPE IF EXISTS comment_v1;
    END
$$;";

        Execute.Sql(sql);
    }
}