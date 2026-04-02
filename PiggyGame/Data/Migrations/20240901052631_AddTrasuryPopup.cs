using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiggyGame.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTrasuryPopup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TreasuryUpdatedPopupShown",
                table: "User",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TreasuryUpdatedPopupShown",
                table: "User");
        }
    }
}
