using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AracKiralamaPortali.API.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationLocationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CurrentLatitude",
                table: "Reservations",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentLocationText",
                table: "Reservations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CurrentLongitude",
                table: "Reservations",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LocationUpdatedAt",
                table: "Reservations",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentLatitude",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CurrentLocationText",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CurrentLongitude",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "LocationUpdatedAt",
                table: "Reservations");
        }
    }
}
