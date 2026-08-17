using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WangenPizza.Migrations
{
    public partial class ExtensionOrderItemId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DeliveryDate",
                table: "Orders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderItemId",
                table: "ExtensionOrderItem",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExtensionOrderItem_OrderItemId",
                table: "ExtensionOrderItem",
                column: "OrderItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExtensionOrderItem_OrderItem_OrderItemId",
                table: "ExtensionOrderItem",
                column: "OrderItemId",
                principalTable: "OrderItem",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExtensionOrderItem_OrderItem_OrderItemId",
                table: "ExtensionOrderItem");

            migrationBuilder.DropIndex(
                name: "IX_ExtensionOrderItem_OrderItemId",
                table: "ExtensionOrderItem");

            migrationBuilder.DropColumn(
                name: "OrderItemId",
                table: "ExtensionOrderItem");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeliveryDate",
                table: "Orders",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }
    }
}
