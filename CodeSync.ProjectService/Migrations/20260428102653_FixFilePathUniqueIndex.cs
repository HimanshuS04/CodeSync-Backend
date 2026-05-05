using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeSync.ProjectService.Migrations
{
    /// <inheritdoc />
    public partial class FixFilePathUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CodeFiles_ProjectId_Path",
                table: "CodeFiles");

            migrationBuilder.CreateIndex(
                name: "IX_CodeFiles_ProjectId_Path",
                table: "CodeFiles",
                columns: new[] { "ProjectId", "Path" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CodeFiles_ProjectId_Path",
                table: "CodeFiles");

            migrationBuilder.CreateIndex(
                name: "IX_CodeFiles_ProjectId_Path",
                table: "CodeFiles",
                columns: new[] { "ProjectId", "Path" },
                unique: true);
        }
    }
}
