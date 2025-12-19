using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSD_E_Learning.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentMaterialProgressTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastAccessedAt",
                table: "StudentCourseProgresses",
                newName: "UpdatedAt");

            migrationBuilder.CreateTable(
                name: "StudentMaterialProgresses",
                columns: table => new
                {
                    StudentMaterialProgressId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    CourseFileId = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentMaterialProgresses", x => x.StudentMaterialProgressId);
                    table.ForeignKey(
                        name: "FK_StudentMaterialProgresses_CourseFiles_CourseFileId",
                        column: x => x.CourseFileId,
                        principalTable: "CourseFiles",
                        principalColumn: "CourseFileId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentMaterialProgresses_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentMaterialProgresses_CourseFileId",
                table: "StudentMaterialProgresses",
                column: "CourseFileId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentMaterialProgresses_StudentId",
                table: "StudentMaterialProgresses",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentMaterialProgresses");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "StudentCourseProgresses",
                newName: "LastAccessedAt");
        }
    }
}
