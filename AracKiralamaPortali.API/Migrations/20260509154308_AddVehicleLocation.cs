using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AracKiralamaPortali.API.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasAccidentHistory",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "InspectionExpiryDate",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "InsuranceExpiryDate",
                table: "Vehicles");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Vehicles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "Vehicles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "District",
                table: "Vehicles");

            migrationBuilder.AddColumn<bool>(
                name: "HasAccidentHistory",
                table: "Vehicles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "InspectionExpiryDate",
                table: "Vehicles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InsuranceExpiryDate",
                table: "Vehicles",
                type: "datetime2",
                nullable: true);
        }
    }
}
