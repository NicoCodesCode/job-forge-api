using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobForgeAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIdempotencyKeyForJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Jobs_OrganizationId",
                table: "Jobs");

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "Jobs",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OrganizationId_IdempotencyKey",
                table: "Jobs",
                columns: new[] { "OrganizationId", "IdempotencyKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Jobs_OrganizationId_IdempotencyKey",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "Jobs");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OrganizationId",
                table: "Jobs",
                column: "OrganizationId");
        }
    }
}
