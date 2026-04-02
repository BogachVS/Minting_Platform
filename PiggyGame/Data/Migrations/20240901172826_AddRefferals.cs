using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiggyGame.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRefferals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReferralCode",
                table: "User",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ReferrerId",
                table: "User",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_ReferrerId",
                table: "User",
                column: "ReferrerId");

            migrationBuilder.AddForeignKey(
                name: "FK_User_User_ReferrerId",
                table: "User",
                column: "ReferrerId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_User_ReferrerId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_ReferrerId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ReferralCode",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ReferrerId",
                table: "User");
        }
    }
}
