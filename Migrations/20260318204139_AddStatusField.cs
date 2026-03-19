using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobAggregatorApi.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "SavedJobs",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "SavedJobs");
        }
    }
}
