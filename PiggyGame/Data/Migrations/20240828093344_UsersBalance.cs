using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiggyGame.Data.Migrations
{
    /// <inheritdoc />
    public partial class UsersBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PigsAmount",
                table: "User",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TicketsAmount",
                table: "User",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PigsAmount",
                table: "User");

            migrationBuilder.DropColumn(
                name: "TicketsAmount",
                table: "User");
        }
    }
}
