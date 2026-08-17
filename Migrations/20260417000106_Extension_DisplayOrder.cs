using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WangenPizza.Migrations
{
    public partial class Extension_DisplayOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Extension",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
UPDATE dbo.Extension SET DisplayOrder = Id WHERE DisplayOrder = 0;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Extension");
        }
    }
}
