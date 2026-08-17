using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WangenPizza.Migrations
{
    public partial class Pickup_typeCart : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pickup_type",
                table: "Orders");

            migrationBuilder.AddColumn<string>(
                name: "Pickup_type",
                table: "ShoppingCarts",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pickup_type",
                table: "ShoppingCarts");

            migrationBuilder.AddColumn<string>(
                name: "Pickup_type",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
