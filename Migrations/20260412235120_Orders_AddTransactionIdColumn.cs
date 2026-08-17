using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WangenPizza.Migrations
{
    public partial class Orders_AddTransactionIdColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Produktions-DBs ohne frühere Anwendung der leeren Migration «UserTransaction»: Spalte fehlt trotz Model.
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Orders', 'TransactionId') IS NULL
    ALTER TABLE dbo.Orders ADD TransactionId bigint NULL;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Orders', 'TransactionId') IS NOT NULL
    ALTER TABLE dbo.Orders DROP COLUMN TransactionId;
");
        }
    }
}
