using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DonationManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ? Indexes for commonly filtered/searched columns
            migrationBuilder.CreateIndex(
                name: "IX_DonationCases_Status_CreatedAt",
                table: "DonationCases",
                columns: new[] { "Status", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DonationCases_CategoryId_Status",
                table: "DonationCases",
                columns: new[] { "CategoryId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DonationCases_TargetAmount",
                table: "DonationCases",
                column: "TargetAmount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DonationCases_Status_CreatedAt",
                table: "DonationCases");

            migrationBuilder.DropIndex(
                name: "IX_DonationCases_CategoryId_Status",
                table: "DonationCases");

            migrationBuilder.DropIndex(
                name: "IX_DonationCases_TargetAmount",
                table: "DonationCases");
        }
    }
}