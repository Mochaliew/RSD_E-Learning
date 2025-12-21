using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSD_E_Learning.Migrations
{
    /// <inheritdoc />
    public partial class FinalExamTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinalExams",
                columns: table => new
                {
                    FinalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TotalMarks = table.Column<int>(type: "int", nullable: true),
                    DeadLine = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PassingMark = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalExams", x => x.FinalId);
                    table.ForeignKey(
                        name: "FK_FinalExams_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinalAttempts",
                columns: table => new
                {
                    AttemptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    FinalId = table.Column<int>(type: "int", nullable: false),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false),
                    IsPassed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalAttempts", x => x.AttemptId);
                    table.ForeignKey(
                        name: "FK_FinalAttempts_FinalExams_FinalId",
                        column: x => x.FinalId,
                        principalTable: "FinalExams",
                        principalColumn: "FinalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FinalAttempts_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinalQuestions",
                columns: table => new
                {
                    QuestionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinalId = table.Column<int>(type: "int", nullable: false),
                    QuestionDetail = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AnswerA = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnswerB = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnswerC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnswerD = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectAnswer = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalQuestions", x => x.QuestionId);
                    table.ForeignKey(
                        name: "FK_FinalQuestions_FinalExams_FinalId",
                        column: x => x.FinalId,
                        principalTable: "FinalExams",
                        principalColumn: "FinalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinalSubmissions",
                columns: table => new
                {
                    FinalSubmissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    SubmittedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Grade = table.Column<double>(type: "float", nullable: true),
                    FinalExamFinalId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalSubmissions", x => x.FinalSubmissionId);
                    table.ForeignKey(
                        name: "FK_FinalSubmissions_FinalExams_FinalExamFinalId",
                        column: x => x.FinalExamFinalId,
                        principalTable: "FinalExams",
                        principalColumn: "FinalId");
                    table.ForeignKey(
                        name: "FK_FinalSubmissions_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "LessonId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FinalSubmissions_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentFinalAnswers",
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
                    table.PrimaryKey("PK_StudentFinalAnswers", x => x.AnswerId);
                    table.ForeignKey(
                        name: "FK_StudentFinalAnswers_FinalAttempts_AttemptId",
                        column: x => x.AttemptId,
                        principalTable: "FinalAttempts",
                        principalColumn: "AttemptId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentFinalAnswers_FinalQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "FinalQuestions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinalAttempts_FinalId",
                table: "FinalAttempts",
                column: "FinalId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalAttempts_StudentId",
                table: "FinalAttempts",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalExams_CourseId",
                table: "FinalExams",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalQuestions_FinalId",
                table: "FinalQuestions",
                column: "FinalId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalSubmissions_FinalExamFinalId",
                table: "FinalSubmissions",
                column: "FinalExamFinalId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalSubmissions_LessonId",
                table: "FinalSubmissions",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalSubmissions_StudentId",
                table: "FinalSubmissions",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentFinalAnswers_AttemptId",
                table: "StudentFinalAnswers",
                column: "AttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentFinalAnswers_QuestionId",
                table: "StudentFinalAnswers",
                column: "QuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinalSubmissions");

            migrationBuilder.DropTable(
                name: "StudentFinalAnswers");

            migrationBuilder.DropTable(
                name: "FinalAttempts");

            migrationBuilder.DropTable(
                name: "FinalQuestions");

            migrationBuilder.DropTable(
                name: "FinalExams");
        }
    }
}
