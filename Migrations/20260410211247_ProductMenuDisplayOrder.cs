using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WangenPizza.Migrations
{
    public partial class ProductMenuDisplayOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Columns may already exist from an earlier deploy — skip if present.
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Product') AND name = N'DisplayOrder')
    ALTER TABLE [dbo].[Product] ADD [DisplayOrder] int NOT NULL CONSTRAINT DF_Product_DisplayOrder DEFAULT 0;
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Orders') AND name = N'PosAcknowledged')
    ALTER TABLE [dbo].[Orders] ADD [PosAcknowledged] bit NOT NULL CONSTRAINT DF_Orders_PosAcknowledged DEFAULT 0;
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Orders') AND name = N'PreparationMinutesEmailed')
    ALTER TABLE [dbo].[Orders] ADD [PreparationMinutesEmailed] int NULL;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Product') AND name = N'DisplayOrder')
BEGIN
    DECLARE @dc sysname = (SELECT dc.name FROM sys.default_constraints dc
        INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
        WHERE dc.parent_object_id = OBJECT_ID(N'dbo.Product') AND c.name = N'DisplayOrder');
    IF @dc IS NOT NULL EXEC(N'ALTER TABLE [dbo].[Product] DROP CONSTRAINT [' + @dc + N'];');
    ALTER TABLE [dbo].[Product] DROP COLUMN [DisplayOrder];
END
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Orders') AND name = N'PosAcknowledged')
BEGIN
    DECLARE @dc2 sysname = (SELECT dc.name FROM sys.default_constraints dc
        INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
        WHERE dc.parent_object_id = OBJECT_ID(N'dbo.Orders') AND c.name = N'PosAcknowledged');
    IF @dc2 IS NOT NULL EXEC(N'ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [' + @dc2 + N'];');
    ALTER TABLE [dbo].[Orders] DROP COLUMN [PosAcknowledged];
END
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Orders') AND name = N'PreparationMinutesEmailed')
    ALTER TABLE [dbo].[Orders] DROP COLUMN [PreparationMinutesEmailed];
");
        }
    }
}
