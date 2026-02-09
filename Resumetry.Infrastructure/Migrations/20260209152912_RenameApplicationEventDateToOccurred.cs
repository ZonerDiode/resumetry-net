using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Resumetry.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameApplicationEventDateToOccurred : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "ApplicationEvents",
                newName: "Occurred");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Occurred",
                table: "ApplicationEvents",
                newName: "Date");
        }
    }
}
