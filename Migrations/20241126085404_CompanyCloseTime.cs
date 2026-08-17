using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WangenPizza.Migrations
{
    public partial class CompanyCloseTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Pausefrom",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pausetill",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Pausetyp",
                table: "CompanyData",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pausefrom",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "Pausetill",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "Pausetyp",
                table: "CompanyData");
        }
    }
}
