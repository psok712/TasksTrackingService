using FluentMigrator;

namespace Route256.Week5.Workshop.PriceCalculator.Dal.Migrations;

[Migration(20230917143600, TransactionBehavior.None)]
public class AddModifiedAtColumnToTaskComments : Migration
{
    public override void Up()
    {
        const string sql = @"
DO $$
BEGIN
    ALTER TABLE task_comments
    ADD COLUMN modified_at TIMESTAMP WITH TIME ZONE NULL;
END
$$;";
        
        Execute.Sql(sql);
    }

    public override void Down()
    {
        const string sql = @"
DO $$
BEGIN
    ALTER TABLE task_comments
    DROP COLUMN IF EXISTS modified_at;
END
$$;";

        Execute.Sql(sql);
    }
}