using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WangenPizza.Migrations
{
    public partial class Category_DisplayOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Category",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Bestehende Zeilen: bisherige Reihenfolge ≈ Id; sonst wären alle 0.
            migrationBuilder.Sql(@"
UPDATE dbo.Category SET DisplayOrder = Id WHERE DisplayOrder = 0;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Category");
        }
    }
}
