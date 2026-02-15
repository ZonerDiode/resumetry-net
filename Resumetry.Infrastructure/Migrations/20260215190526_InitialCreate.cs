using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Resumetry.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Recruiters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Company = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recruiters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Company = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Position = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Salary = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TopJob = table.Column<bool>(type: "INTEGER", nullable: false),
                    SourcePage = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ReviewPage = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    LoginNotes = table.Column<string>(type: "TEXT", nullable: true),
                    RecruiterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobApplications_Recruiters_RecruiterId",
                        column: x => x.RecruiterId,
                        principalTable: "Recruiters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Occurred = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    JobApplicationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationEvents_JobApplications_JobApplicationId",
                        column: x => x.JobApplicationId,
                        principalTable: "JobApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Occurred = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    JobApplicationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationStatuses_JobApplications_JobApplicationId",
                        column: x => x.JobApplicationId,
                        principalTable: "JobApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationEvents_JobApplicationId",
                table: "ApplicationEvents",
                column: "JobApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStatuses_JobApplicationId",
                table: "ApplicationStatuses",
                column: "JobApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_RecruiterId",
                table: "JobApplications",
                column: "RecruiterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationEvents");

            migrationBuilder.DropTable(
                name: "ApplicationStatuses");

            migrationBuilder.DropTable(
                name: "JobApplications");

            migrationBuilder.DropTable(
                name: "Recruiters");
        }
    }
}
