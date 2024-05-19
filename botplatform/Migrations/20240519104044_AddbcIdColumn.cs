using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace botplatform.Migrations
{
    /// <inheritdoc />
    public partial class AddbcIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "bcId",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bcId",
                table: "Users");
        }
    }
}
