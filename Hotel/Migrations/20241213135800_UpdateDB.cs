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
            migrationBuilder.DropForeignKey(
                name: "FK_Users_tokens_TokenId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_TokenId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TokenId",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_tokens_UserID",
                table: "tokens",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_tokens_Users_UserID",
                table: "tokens",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tokens_Users_UserID",
                table: "tokens");

            migrationBuilder.DropIndex(
                name: "IX_tokens_UserID",
                table: "tokens");

            migrationBuilder.AddColumn<string>(
                name: "TokenId",
                table: "Users",
                type: "nvarchar(100)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TokenId",
                table: "Users",
                column: "TokenId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_tokens_TokenId",
                table: "Users",
                column: "TokenId",
                principalTable: "tokens",
                principalColumn: "Id");
        }
    }
}
