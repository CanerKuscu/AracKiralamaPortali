using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AracKiralamaPortali.API.Migrations
{
    /// <inheritdoc />
    public partial class RestoreVehicleFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
