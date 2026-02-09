using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DonationManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedCategoriesAndTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed Categories
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Name", "Slug" },
                values: new object[,]
                {
                    { "Medical", "medical" },
                    { "Education", "education" },
                    { "Emergency Relief", "emergency-relief" },
                    { "Community Development", "community-development" },
                    { "Housing", "housing" },
                    { "Food & Nutrition", "food-nutrition" },
                    { "Mental Health", "mental-health" },
                    { "Child Welfare", "child-welfare" }
                });

            // Seed Tags
            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Name", "Slug" },
                values: new object[,]
                {
                    { "Urgent", "urgent" },
                    { "Long-term Support", "long-term-support" },
                    { "Family", "family" },
                    { "Children", "children" },
                    { "Elderly", "elderly" },
                    { "Disability", "disability" },
                    { "Poverty", "poverty" },
                    { "Natural Disaster", "natural-disaster" },
                    { "Disease", "disease" },
                    { "Surgery", "surgery" },
                    { "Scholarships", "scholarships" },
                    { "Skills Training", "skills-training" },
                    { "Housing Crisis", "housing-crisis" },
                    { "Clean Water", "clean-water" },
                    { "Community Support", "community-support" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7, 8 });
        }
    }
}