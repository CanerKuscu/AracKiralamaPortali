using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AracKiralamaPortali.API.Migrations
{
    /// <inheritdoc />
    public partial class AddIsReturnConfirmedToReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReturnConfirmed",
                table: "Reservations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReturnConfirmed",
                table: "Reservations");
        }
    }
}
