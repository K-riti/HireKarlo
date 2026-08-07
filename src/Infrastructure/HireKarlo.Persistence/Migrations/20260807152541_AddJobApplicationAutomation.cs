using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HireKarlo.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddJobApplicationAutomation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoTailorResume",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AutomationApplicationsThisMonth",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "AutomationEnabled",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AutomationHistory",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DailyApplicationTarget",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAutomationRunAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MinimumMatchScoreForAutomation",
                table: "Users",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PreferredResumeIdForAutomation",
                table: "Users",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoTailorResume",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AutomationApplicationsThisMonth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AutomationEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AutomationHistory",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DailyApplicationTarget",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastAutomationRunAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MinimumMatchScoreForAutomation",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PreferredResumeIdForAutomation",
                table: "Users");
        }
    }
}
