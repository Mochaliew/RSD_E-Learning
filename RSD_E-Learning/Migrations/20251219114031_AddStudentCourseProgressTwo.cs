using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSD_E_Learning.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentCourseProgressTwo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentCourseProgresses",
                columns: table => new
                {
                    ProgressId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    CourseFileId = table.Column<int>(type: "int", nullable: false),
                    FirstAccessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentCourseProgresses", x => x.ProgressId);
                    table.ForeignKey(
                        name: "FK_StudentCourseProgresses_CourseFiles_CourseFileId",
                        column: x => x.CourseFileId,
                        principalTable: "CourseFiles",
                        principalColumn: "CourseFileId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentCourseProgresses_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentCourseProgresses_CourseFileId",
                table: "StudentCourseProgresses",
                column: "CourseFileId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCourseProgresses_StudentId",
                table: "StudentCourseProgresses",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentCourseProgresses");
        }
    }
}
