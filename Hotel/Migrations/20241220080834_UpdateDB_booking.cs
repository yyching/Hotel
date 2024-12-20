using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hotel.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDB_booking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_ServiceBooking_ServiceBookingID",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ServiceBookingID",
                table: "Bookings");

            migrationBuilder.AlterColumn<string>(
                name: "ServiceBookingID",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ServiceBookingID",
                table: "Bookings",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ServiceBookingID",
                table: "Bookings",
                column: "ServiceBookingID");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_ServiceBooking_ServiceBookingID",
                table: "Bookings",
                column: "ServiceBookingID",
                principalTable: "ServiceBooking",
                principalColumn: "ID");
        }
    }
}
