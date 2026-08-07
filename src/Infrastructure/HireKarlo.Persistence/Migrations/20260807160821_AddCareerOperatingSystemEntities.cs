using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HireKarlo.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCareerOperatingSystemEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CareerGoalSummary",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasCompletedOnboarding",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OnboardingCompletedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DreamCompanyId",
                table: "Matches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OpportunityMatchId",
                table: "Matches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhyThisDreamCompany",
                table: "Matches",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SourcePlatform",
                table: "InterviewDigestEntries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Snippet",
                table: "InterviewDigestEntries",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LlmSummary",
                table: "InterviewDigestEntries",
                type: "character varying(3000)",
                maxLength: 3000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KeyTakeaways",
                table: "InterviewDigestEntries",
                type: "character varying(3000)",
                maxLength: 3000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContentCategory",
                table: "InterviewDigestEntries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentEmbedding",
                table: "InterviewDigestEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DreamCompanyId",
                table: "InterviewDigestEntries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Frequency",
                table: "InterviewDigestEntries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Relevance",
                table: "InterviewDigestEntries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "InterviewDigestEntries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "CareerProgress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckinDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MilestoneType = table.Column<int>(type: "integer", nullable: false),
                    MilestoneDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RelatedDreamCompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    ImpactOnDreamCompanies = table.Column<double>(type: "double precision", nullable: false),
                    Evidence = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SkillsUnlocked = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CareerProgress_DreamCompanies_RelatedDreamCompanyId",
                        column: x => x.RelatedDreamCompanyId,
                        principalTable: "DreamCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CareerProgress_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DreamCompanyMatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DreamCompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentMatchPercentage = table.Column<double>(type: "double precision", nullable: false),
                    TargetMatchPercentage = table.Column<double>(type: "double precision", nullable: false),
                    MatchBreakdown = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    GapAnalysis = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Recommendations = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    LastCalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextRecalculateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DreamCompanyMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DreamCompanyMatches_DreamCompanies_DreamCompanyId",
                        column: x => x.DreamCompanyId,
                        principalTable: "DreamCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DreamCompanyMatches_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityMatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    DreamCompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchPercentage = table.Column<double>(type: "double precision", nullable: false),
                    DiscoveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NotificationSent = table.Column<bool>(type: "boolean", nullable: false),
                    ExplanationForMatch = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    MatchingFactors = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    MissingFactors = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SkillsAlreadyHave = table.Column<int>(type: "integer", nullable: false),
                    SkillsToLearn = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityMatches_DreamCompanies_DreamCompanyId",
                        column: x => x.DreamCompanyId,
                        principalTable: "DreamCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OpportunityMatches_JobListings_JobListingId",
                        column: x => x.JobListingId,
                        principalTable: "JobListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OpportunityMatches_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReferralTargets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DreamCompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LinkedInUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SimilarityScore = table.Column<double>(type: "double precision", nullable: false),
                    BackgroundSimilarity = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SuggestedOutreach = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DraftMessage = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    OutreachSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FollowUpDueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralTargets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReferralTargets_DreamCompanies_DreamCompanyId",
                        column: x => x.DreamCompanyId,
                        principalTable: "DreamCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReferralTargets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SkillGraphs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Proficiency = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AcquiredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Evidence = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EmbeddingVector = table.Column<string>(type: "text", nullable: true),
                    ImpactMetrics = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillGraphs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillGraphs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SkillGapRecommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillGraphId = table.Column<Guid>(type: "uuid", nullable: true),
                    DreamCompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecommendedSkill = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Reasoning = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    LearningResources = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ProjectIdea = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ImpactSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EstimatedHours = table.Column<int>(type: "integer", nullable: false),
                    TargetCompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROIScore = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillGapRecommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillGapRecommendations_DreamCompanies_DreamCompanyId",
                        column: x => x.DreamCompanyId,
                        principalTable: "DreamCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SkillGapRecommendations_SkillGraphs_SkillGraphId",
                        column: x => x.SkillGraphId,
                        principalTable: "SkillGraphs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SkillGapRecommendations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_DreamCompanyId",
                table: "Matches",
                column: "DreamCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_OpportunityMatchId",
                table: "Matches",
                column: "OpportunityMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewDigestEntries_DreamCompanyId",
                table: "InterviewDigestEntries",
                column: "DreamCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewDigestEntries_Relevance",
                table: "InterviewDigestEntries",
                column: "Relevance");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewDigestEntries_UserId",
                table: "InterviewDigestEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CareerProgress_CheckinDate",
                table: "CareerProgress",
                column: "CheckinDate");

            migrationBuilder.CreateIndex(
                name: "IX_CareerProgress_MilestoneType",
                table: "CareerProgress",
                column: "MilestoneType");

            migrationBuilder.CreateIndex(
                name: "IX_CareerProgress_RelatedDreamCompanyId",
                table: "CareerProgress",
                column: "RelatedDreamCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CareerProgress_UserId",
                table: "CareerProgress",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DreamCompanyMatches_CurrentMatchPercentage",
                table: "DreamCompanyMatches",
                column: "CurrentMatchPercentage");

            migrationBuilder.CreateIndex(
                name: "IX_DreamCompanyMatches_DreamCompanyId",
                table: "DreamCompanyMatches",
                column: "DreamCompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DreamCompanyMatches_UserId",
                table: "DreamCompanyMatches",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityMatches_DiscoveredAt",
                table: "OpportunityMatches",
                column: "DiscoveredAt");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityMatches_DreamCompanyId",
                table: "OpportunityMatches",
                column: "DreamCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityMatches_JobListingId",
                table: "OpportunityMatches",
                column: "JobListingId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityMatches_MatchPercentage",
                table: "OpportunityMatches",
                column: "MatchPercentage");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityMatches_UserId",
                table: "OpportunityMatches",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralTargets_DreamCompanyId",
                table: "ReferralTargets",
                column: "DreamCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralTargets_SimilarityScore",
                table: "ReferralTargets",
                column: "SimilarityScore");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralTargets_Status",
                table: "ReferralTargets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralTargets_UserId",
                table: "ReferralTargets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillGapRecommendations_DreamCompanyId",
                table: "SkillGapRecommendations",
                column: "DreamCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillGapRecommendations_Priority",
                table: "SkillGapRecommendations",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_SkillGapRecommendations_ROIScore",
                table: "SkillGapRecommendations",
                column: "ROIScore");

            migrationBuilder.CreateIndex(
                name: "IX_SkillGapRecommendations_SkillGraphId",
                table: "SkillGapRecommendations",
                column: "SkillGraphId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillGapRecommendations_UserId",
                table: "SkillGapRecommendations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillGraphs_Category",
                table: "SkillGraphs",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_SkillGraphs_SkillName",
                table: "SkillGraphs",
                column: "SkillName");

            migrationBuilder.CreateIndex(
                name: "IX_SkillGraphs_UserId",
                table: "SkillGraphs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewDigestEntries_DreamCompanies_DreamCompanyId",
                table: "InterviewDigestEntries",
                column: "DreamCompanyId",
                principalTable: "DreamCompanies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewDigestEntries_Users_UserId",
                table: "InterviewDigestEntries",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_DreamCompanies_DreamCompanyId",
                table: "Matches",
                column: "DreamCompanyId",
                principalTable: "DreamCompanies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_OpportunityMatches_OpportunityMatchId",
                table: "Matches",
                column: "OpportunityMatchId",
                principalTable: "OpportunityMatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InterviewDigestEntries_DreamCompanies_DreamCompanyId",
                table: "InterviewDigestEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_InterviewDigestEntries_Users_UserId",
                table: "InterviewDigestEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_DreamCompanies_DreamCompanyId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_OpportunityMatches_OpportunityMatchId",
                table: "Matches");

            migrationBuilder.DropTable(
                name: "CareerProgress");

            migrationBuilder.DropTable(
                name: "DreamCompanyMatches");

            migrationBuilder.DropTable(
                name: "OpportunityMatches");

            migrationBuilder.DropTable(
                name: "ReferralTargets");

            migrationBuilder.DropTable(
                name: "SkillGapRecommendations");

            migrationBuilder.DropTable(
                name: "SkillGraphs");

            migrationBuilder.DropIndex(
                name: "IX_Matches_DreamCompanyId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_OpportunityMatchId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_InterviewDigestEntries_DreamCompanyId",
                table: "InterviewDigestEntries");

            migrationBuilder.DropIndex(
                name: "IX_InterviewDigestEntries_Relevance",
                table: "InterviewDigestEntries");

            migrationBuilder.DropIndex(
                name: "IX_InterviewDigestEntries_UserId",
                table: "InterviewDigestEntries");

            migrationBuilder.DropColumn(
                name: "CareerGoalSummary",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "HasCompletedOnboarding",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OnboardingCompletedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DreamCompanyId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "OpportunityMatchId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "WhyThisDreamCompany",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "ContentCategory",
                table: "InterviewDigestEntries");

            migrationBuilder.DropColumn(
                name: "ContentEmbedding",
                table: "InterviewDigestEntries");

            migrationBuilder.DropColumn(
                name: "DreamCompanyId",
                table: "InterviewDigestEntries");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "InterviewDigestEntries");

            migrationBuilder.DropColumn(
                name: "Relevance",
                table: "InterviewDigestEntries");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "InterviewDigestEntries");

            migrationBuilder.AlterColumn<string>(
                name: "SourcePlatform",
                table: "InterviewDigestEntries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Snippet",
                table: "InterviewDigestEntries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(5000)",
                oldMaxLength: 5000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LlmSummary",
                table: "InterviewDigestEntries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(3000)",
                oldMaxLength: 3000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KeyTakeaways",
                table: "InterviewDigestEntries",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(3000)",
                oldMaxLength: 3000,
                oldNullable: true);
        }
    }
}
