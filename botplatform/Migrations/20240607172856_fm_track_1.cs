using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace botplatform.Migrations
{
    /// <inheritdoc />
    public partial class fm_track_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "autoreply_date",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "chat_delete_date",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "first_msg_id",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "first_msg_rcvd_date",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "first_msg_rep_date",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "is_chat_deleted",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_first_msg_rep",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "was_autoreply",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "autoreply_date",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "chat_delete_date",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "first_msg_id",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "first_msg_rcvd_date",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "first_msg_rep_date",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "is_chat_deleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "is_first_msg_rep",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "was_autoreply",
                table: "Users");
        }
    }
}
