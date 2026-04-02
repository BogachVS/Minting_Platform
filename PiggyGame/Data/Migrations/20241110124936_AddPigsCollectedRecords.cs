using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiggyGame.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPigsCollectedRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MaxPigsAmount",
                table: "User",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "NewGlobalRecordPopupShown",
                table: "User",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "NewRecordPopupShown",
                table: "User",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxPigsAmount",
                table: "User");

            migrationBuilder.DropColumn(
                name: "NewGlobalRecordPopupShown",
                table: "User");

            migrationBuilder.DropColumn(
                name: "NewRecordPopupShown",
                table: "User");
        }
    }
}
