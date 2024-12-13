using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hotel.Migrations
{
    /// <inheritdoc />
    public partial class UpdateServiceBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_ServiceBooking_ServiceBookingID",
                table: "Bookings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServiceBooking",
                table: "ServiceBooking");

            migrationBuilder.AddColumn<string>(
                name: "ID",
                table: "ServiceBooking",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ServiceBookingID",
                table: "Bookings",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServiceBooking",
                table: "ServiceBooking",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_ServiceBooking_ServiceBookingID",
                table: "Bookings",
                column: "ServiceBookingID",
                principalTable: "ServiceBooking",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_ServiceBooking_ServiceBookingID",
                table: "Bookings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServiceBooking",
                table: "ServiceBooking");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "ServiceBooking");

            migrationBuilder.AlterColumn<string>(
                name: "ServiceBookingID",
                table: "Bookings",
                type: "nvarchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServiceBooking",
                table: "ServiceBooking",
                column: "ServiceBookingID");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_ServiceBooking_ServiceBookingID",
                table: "Bookings",
                column: "ServiceBookingID",
                principalTable: "ServiceBooking",
                principalColumn: "ServiceBookingID");
        }
    }
}
