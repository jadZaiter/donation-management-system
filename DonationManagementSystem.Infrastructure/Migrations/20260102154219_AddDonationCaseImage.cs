using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DonationManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDonationCaseImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "DonationCases",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "DonationCases");
        }
    }
}
