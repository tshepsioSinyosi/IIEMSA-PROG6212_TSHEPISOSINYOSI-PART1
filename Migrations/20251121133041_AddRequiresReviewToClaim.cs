using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractClaimSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddRequiresReviewToClaim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiresReview",
                table: "Claims",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiresReview",
                table: "Claims");
        }
    }
}
