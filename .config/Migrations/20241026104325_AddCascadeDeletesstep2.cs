using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WangenPizza.Migrations
{
    public partial class AddCascadeDeletesstep2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_ShoppingCarts_ShoppingCartId",
                table: "OrderItem");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_ShoppingCarts_ShoppingCartId",
                table: "OrderItem",
                column: "ShoppingCartId",
                principalTable: "ShoppingCarts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_ShoppingCarts_ShoppingCartId",
                table: "OrderItem");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_ShoppingCarts_ShoppingCartId",
                table: "OrderItem",
                column: "ShoppingCartId",
                principalTable: "ShoppingCarts",
                principalColumn: "Id");
        }
    }
}
