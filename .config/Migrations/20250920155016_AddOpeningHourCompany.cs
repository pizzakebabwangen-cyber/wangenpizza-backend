using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WangenPizza.Migrations
{
    public partial class AddOpeningHourCompany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpeningHours");

            migrationBuilder.AddColumn<string>(
                name: "FridayFrom1",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FridayFrom2",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FridayTill1",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FridayTill2",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MondayFrom1",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MondayFrom2",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MondayTill1",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MondayTill2",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SaturdayFrom1",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SaturdayFrom2",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SaturdayTill1",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SaturdayTill2",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SundayFrom1",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SundayFrom2",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SundayTill1",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SundayTill2",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThursdayFrom1",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThursdayFrom2",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThursdayTill1",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThursdayTill2",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TuesdayFrom1",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TuesdayFrom2",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TuesdayTill1",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TuesdayTill2",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WednesdayFrom1",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WednesdayFrom2",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WednesdayTill1",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WednesdayTill2",
                table: "CompanyData",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FridayFrom1",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "FridayFrom2",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "FridayTill1",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "FridayTill2",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "MondayFrom1",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "MondayFrom2",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "MondayTill1",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "MondayTill2",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "SaturdayFrom1",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "SaturdayFrom2",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "SaturdayTill1",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "SaturdayTill2",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "SundayFrom1",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "SundayFrom2",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "SundayTill1",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "SundayTill2",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "ThursdayFrom1",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "ThursdayFrom2",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "ThursdayTill1",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "ThursdayTill2",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "TuesdayFrom1",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "TuesdayFrom2",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "TuesdayTill1",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "TuesdayTill2",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "WednesdayFrom1",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "WednesdayFrom2",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "WednesdayTill1",
                table: "CompanyData");

            migrationBuilder.DropColumn(
                name: "WednesdayTill2",
                table: "CompanyData");

            migrationBuilder.CreateTable(
                name: "OpeningHours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyDataId = table.Column<int>(type: "int", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false),
                    From1 = table.Column<TimeSpan>(type: "time", nullable: true),
                    From2 = table.Column<TimeSpan>(type: "time", nullable: true),
                    To1 = table.Column<TimeSpan>(type: "time", nullable: true),
                    To2 = table.Column<TimeSpan>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpeningHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpeningHours_CompanyData_CompanyDataId",
                        column: x => x.CompanyDataId,
                        principalTable: "CompanyData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpeningHours_CompanyDataId_Day",
                table: "OpeningHours",
                columns: new[] { "CompanyDataId", "Day" },
                unique: true);
        }
    }
}
