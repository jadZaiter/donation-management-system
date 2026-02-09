using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DonationManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryTagAndDonationCaseTag1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Create Tables FIRST (before adding FK constraint)
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            // Step 2: Create Indexes on Slugs BEFORE seeding
            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Slug",
                table: "Tags",
                column: "Slug",
                unique: true);

            // Step 3: Seed Categories (8 items)
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Name", "Slug" },
                values: new object[,]
                {
                    { "Medical", "medical" },
                    { "Education", "education" },
                    { "Emergency", "emergency" },
                    { "Housing", "housing" },
                    { "Business", "business" },
                    { "Community", "community" },
                    { "Disability", "disability" },
                    { "Family Support", "family-support" }
                });

            // Step 4: Seed Tags (15 items)
            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Name", "Slug" },
                values: new object[,]
                {
                    { "Urgent", "urgent" },
                    { "Verified", "verified" },
                    { "Long-term", "long-term" },
                    { "Mental Health", "mental-health" },
                    { "Physical Health", "physical-health" },
                    { "Children", "children" },
                    { "Elderly", "elderly" },
                    { "Job Training", "job-training" },
                    { "Scholarship", "scholarship" },
                    { "Disaster Relief", "disaster-relief" },
                    { "Rent Assistance", "rent-assistance" },
                    { "Food Security", "food-security" },
                    { "Healthcare", "healthcare" },
                    { "Accessibility", "accessibility" },
                    { "Community Project", "community-project" }
                });

            // Step 5: Add CategoryId column to DonationCases
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "DonationCases",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // Step 6: Update existing DonationCases to have valid CategoryId
            migrationBuilder.Sql("UPDATE DonationCases SET CategoryId = 1");

            // Step 7: Create DonationCaseTags table
            migrationBuilder.CreateTable(
                name: "DonationCaseTags",
                columns: table => new
                {
                    DonationCaseId = table.Column<int>(type: "int", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonationCaseTags", x => new { x.DonationCaseId, x.TagId });
                    table.ForeignKey(
                        name: "FK_DonationCaseTags_DonationCases_DonationCaseId",
                        column: x => x.DonationCaseId,
                        principalTable: "DonationCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DonationCaseTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Step 8: Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_DonationCases_CategoryId",
                table: "DonationCases",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationCaseTags_TagId",
                table: "DonationCaseTags",
                column: "TagId");

            // Step 9: Add Foreign Key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_DonationCases_Categories_CategoryId",
                table: "DonationCases",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DonationCases_Categories_CategoryId",
                table: "DonationCases");

            migrationBuilder.DropTable(
                name: "DonationCaseTags");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_DonationCases_CategoryId",
                table: "DonationCases");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "DonationCases");
        }
    }
}