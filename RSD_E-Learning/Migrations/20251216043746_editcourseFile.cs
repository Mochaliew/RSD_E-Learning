using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSD_E_Learning.Migrations
{
    /// <inheritdoc />
    public partial class editcourseFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseFiles_Teachers_TeacherId",
                table: "CourseFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseFiles_Teachers_TeacherId1",
                table: "CourseFiles");

            migrationBuilder.DropIndex(
                name: "IX_CourseFiles_TeacherId1",
                table: "CourseFiles");

            migrationBuilder.DropColumn(
                name: "TeacherId1",
                table: "CourseFiles");

            migrationBuilder.AlterColumn<int>(
                name: "TeacherId",
                table: "CourseFiles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseFiles_Teachers_TeacherId",
                table: "CourseFiles",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseFiles_Teachers_TeacherId",
                table: "CourseFiles");

            migrationBuilder.AlterColumn<int>(
                name: "TeacherId",
                table: "CourseFiles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeacherId1",
                table: "CourseFiles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseFiles_TeacherId1",
                table: "CourseFiles",
                column: "TeacherId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseFiles_Teachers_TeacherId",
                table: "CourseFiles",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "TeacherId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseFiles_Teachers_TeacherId1",
                table: "CourseFiles",
                column: "TeacherId1",
                principalTable: "Teachers",
                principalColumn: "TeacherId");
        }
    }
}
