using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSD_E_Learning.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoGradingTables : Migration
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

            migrationBuilder.CreateTable(
                name: "AssessmentAttempts",
                columns: table => new
                {
                    AttemptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    AssessmentId = table.Column<int>(type: "int", nullable: false),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false),
                    IsPassed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentAttempts", x => x.AttemptId);
                    table.ForeignKey(
                        name: "FK_AssessmentAttempts_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "AssessmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentAttempts_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentAnswers",
                columns: table => new
                {
                    AnswerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttemptId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    SelectedAnswer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAnswers", x => x.AnswerId);
                    table.ForeignKey(
                        name: "FK_StudentAnswers_AssessmentAttempts_AttemptId",
                        column: x => x.AttemptId,
                        principalTable: "AssessmentAttempts",
                        principalColumn: "AttemptId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentAnswers_AssessmentQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "AssessmentQuestions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAttempts_AssessmentId",
                table: "AssessmentAttempts",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAttempts_StudentId",
                table: "AssessmentAttempts",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnswers_AttemptId",
                table: "StudentAnswers",
                column: "AttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnswers_QuestionId",
                table: "StudentAnswers",
                column: "QuestionId");

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

            migrationBuilder.DropTable(
                name: "StudentAnswers");

            migrationBuilder.DropTable(
                name: "AssessmentAttempts");

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
