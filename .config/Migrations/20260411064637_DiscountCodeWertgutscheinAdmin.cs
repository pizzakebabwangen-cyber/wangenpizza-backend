using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WangenPizza.Migrations
{
    public partial class DiscountCodeWertgutscheinAdmin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "DiscountCode",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "DiscountCode",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWertgutschein",
                table: "DiscountCode",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "DiscountCode",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalValueChf",
                table: "DiscountCode",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "DiscountCode");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "DiscountCode");

            migrationBuilder.DropColumn(
                name: "IsWertgutschein",
                table: "DiscountCode");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "DiscountCode");

            migrationBuilder.DropColumn(
                name: "OriginalValueChf",
                table: "DiscountCode");
        }
    }
}
