using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSD_E_Learning.Migrations
{
    /// <inheritdoc />
    public partial class ReworkLesson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseFiles_Courses_CourseId",
                table: "CourseFiles");

            migrationBuilder.AlterColumn<int>(
                name: "CourseId",
                table: "CourseFiles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "LessonId",
                table: "CourseFiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CourseFiles_LessonId",
                table: "CourseFiles",
                column: "LessonId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseFiles_Courses_CourseId",
                table: "CourseFiles",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseFiles_Lessons_LessonId",
                table: "CourseFiles",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "LessonId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseFiles_Courses_CourseId",
                table: "CourseFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseFiles_Lessons_LessonId",
                table: "CourseFiles");

            migrationBuilder.DropIndex(
                name: "IX_CourseFiles_LessonId",
                table: "CourseFiles");

            migrationBuilder.DropColumn(
                name: "LessonId",
                table: "CourseFiles");

            migrationBuilder.AlterColumn<int>(
                name: "CourseId",
                table: "CourseFiles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseFiles_Courses_CourseId",
                table: "CourseFiles",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
