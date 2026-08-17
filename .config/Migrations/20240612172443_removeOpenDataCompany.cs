using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WangenPizza.Migrations
{
    public partial class removeOpenDataCompany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Open1",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "Open2",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "Open3",
                table: "CompanyData");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Open1",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Open2",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Open3",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
