using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementCenter.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStudentCredentialsAndLinkToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "password_hash",
                table: "students");

            migrationBuilder.DropColumn(
                name: "user_name",
                table: "students");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "students",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_students_ApplicationUserId",
                table: "students",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_students_AspNetUsers_ApplicationUserId",
                table: "students",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_students_AspNetUsers_ApplicationUserId",
                table: "students");

            migrationBuilder.DropIndex(
                name: "IX_students_ApplicationUserId",
                table: "students");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "students");

            migrationBuilder.AddColumn<string>(
                name: "password_hash",
                table: "students",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "user_name",
                table: "students",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
