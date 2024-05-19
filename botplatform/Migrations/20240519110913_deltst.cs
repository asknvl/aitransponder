using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace botplatform.Migrations
{
    /// <inheritdoc />
    public partial class deltst : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "testColumn",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "testColumn",
                table: "Users",
                type: "TEXT",
                nullable: true);
        }
    }
}
