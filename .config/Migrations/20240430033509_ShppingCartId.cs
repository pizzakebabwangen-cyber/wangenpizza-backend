using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WangenPizza.Migrations
{
    public partial class ShppingCartId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShoppingCartId",
                table: "OrderItem",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_ShoppingCartId",
                table: "OrderItem",
                column: "ShoppingCartId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_ShoppingCarts_ShoppingCartId",
                table: "OrderItem",
                column: "ShoppingCartId",
                principalTable: "ShoppingCarts",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_ShoppingCarts_ShoppingCartId",
                table: "OrderItem");

            migrationBuilder.DropIndex(
                name: "IX_OrderItem_ShoppingCartId",
                table: "OrderItem");

            migrationBuilder.DropColumn(
                name: "ShoppingCartId",
                table: "OrderItem");
        }
    }
}
