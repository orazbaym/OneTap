using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project1.Migrations
{
    /// <inheritdoc />
    public partial class ComputerClubIdRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_ComputerClubs_ComputerClubId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ComputerClubId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ComputerClubId",
                table: "Bookings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ComputerClubId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ComputerClubId",
                table: "Bookings",
                column: "ComputerClubId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_ComputerClubs_ComputerClubId",
                table: "Bookings",
                column: "ComputerClubId",
                principalTable: "ComputerClubs",
                principalColumn: "Id");
        }
    }
}
