using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hotel.Migrations
{
    /// <inheritdoc />
    public partial class Update_Token : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tokens_Users_UserID",
                table: "tokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tokens",
                table: "tokens");

            migrationBuilder.RenameTable(
                name: "tokens",
                newName: "Tokens");

            migrationBuilder.RenameIndex(
                name: "IX_tokens_UserID",
                table: "Tokens",
                newName: "IX_Tokens_UserID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tokens",
                table: "Tokens",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tokens_Users_UserID",
                table: "Tokens",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tokens_Users_UserID",
                table: "Tokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tokens",
                table: "Tokens");

            migrationBuilder.RenameTable(
                name: "Tokens",
                newName: "tokens");

            migrationBuilder.RenameIndex(
                name: "IX_Tokens_UserID",
                table: "tokens",
                newName: "IX_tokens_UserID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tokens",
                table: "tokens",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_tokens_Users_UserID",
                table: "tokens",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
