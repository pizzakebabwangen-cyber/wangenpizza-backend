using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WangenPizza.Migrations
{
    public partial class NumberOfPeopleReservation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "PostBox",
                table: "Reservation");

            migrationBuilder.RenameColumn(
                name: "Street",
                table: "Reservation",
                newName: "NumberOfPeople");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NumberOfPeople",
                table: "Reservation",
                newName: "Street");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Reservation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostBox",
                table: "Reservation",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
