using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hotel.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PricePerNight",
                table: "Rooms");

            migrationBuilder.AddColumn<string>(
                name: "Capacity",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "PricePerNight",
                table: "Categories",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Services",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Size",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "PricePerNight",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Services",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "Categories");

            migrationBuilder.AddColumn<double>(
                name: "PricePerNight",
                table: "Rooms",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
