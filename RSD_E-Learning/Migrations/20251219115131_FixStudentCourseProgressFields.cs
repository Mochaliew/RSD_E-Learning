using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSD_E_Learning.Migrations
{
    /// <inheritdoc />
    public partial class FixStudentCourseProgressFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentCourseProgresses_CourseFiles_CourseFileId",
                table: "StudentCourseProgresses");

            migrationBuilder.DropIndex(
                name: "IX_StudentCourseProgresses_CourseFileId",
                table: "StudentCourseProgresses");

            migrationBuilder.DropColumn(
                name: "FirstAccessedAt",
                table: "StudentCourseProgresses");

            migrationBuilder.RenameColumn(
                name: "CourseFileId",
                table: "StudentCourseProgresses",
                newName: "ProgressPercentage");

            migrationBuilder.RenameColumn(
                name: "ProgressId",
                table: "StudentCourseProgresses",
                newName: "StudentCourseProgressId");

            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "StudentCourseProgresses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_StudentCourseProgresses_CourseId",
                table: "StudentCourseProgresses",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCourseProgresses_Courses_CourseId",
                table: "StudentCourseProgresses",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentCourseProgresses_Courses_CourseId",
                table: "StudentCourseProgresses");

            migrationBuilder.DropIndex(
                name: "IX_StudentCourseProgresses_CourseId",
                table: "StudentCourseProgresses");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "StudentCourseProgresses");

            migrationBuilder.RenameColumn(
                name: "ProgressPercentage",
                table: "StudentCourseProgresses",
                newName: "CourseFileId");

            migrationBuilder.RenameColumn(
                name: "StudentCourseProgressId",
                table: "StudentCourseProgresses",
                newName: "ProgressId");

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstAccessedAt",
                table: "StudentCourseProgresses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_StudentCourseProgresses_CourseFileId",
                table: "StudentCourseProgresses",
                column: "CourseFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCourseProgresses_CourseFiles_CourseFileId",
                table: "StudentCourseProgresses",
                column: "CourseFileId",
                principalTable: "CourseFiles",
                principalColumn: "CourseFileId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
