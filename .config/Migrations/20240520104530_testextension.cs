using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WangenPizza.Migrations
{
    public partial class testextension : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CartItemId",
                table: "Extension",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Extension",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Extension_CartItemId",
                table: "Extension",
                column: "CartItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Extension_ProductId",
                table: "Extension",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Extension_CartItems_CartItemId",
                table: "Extension",
                column: "CartItemId",
                principalTable: "CartItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Extension_Product_ProductId",
                table: "Extension",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Extension_CartItems_CartItemId",
                table: "Extension");

            migrationBuilder.DropForeignKey(
                name: "FK_Extension_Product_ProductId",
                table: "Extension");

            migrationBuilder.DropIndex(
                name: "IX_Extension_CartItemId",
                table: "Extension");

            migrationBuilder.DropIndex(
                name: "IX_Extension_ProductId",
                table: "Extension");

            migrationBuilder.DropColumn(
                name: "CartItemId",
                table: "Extension");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Extension");
        }
    }
}
