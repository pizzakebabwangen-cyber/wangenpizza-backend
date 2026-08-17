using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WangenPizza.Migrations
{
    public partial class TodayBonus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Offer");

            migrationBuilder.AddColumn<string>(
                name: "Description1",
                table: "Offer",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description2",
                table: "Offer",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description3",
                table: "Offer",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TodayBonus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhotoName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodayBonus", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TodayBonus");

            migrationBuilder.DropColumn(
                name: "Description1",
                table: "Offer");

            migrationBuilder.DropColumn(
                name: "Description2",
                table: "Offer");

            migrationBuilder.DropColumn(
                name: "Description3",
                table: "Offer");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Offer",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
