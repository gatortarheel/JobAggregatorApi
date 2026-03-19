using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobAggregatorApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchScoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MatchRationale",
                table: "SavedJobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MatchScore",
                table: "SavedJobs",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatchRationale",
                table: "SavedJobs");

            migrationBuilder.DropColumn(
                name: "MatchScore",
                table: "SavedJobs");
        }
    }
}
