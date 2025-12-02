using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSD_E_Learning.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PDFFilePath",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "isAproved",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "Feedback",
                table: "AssessmentSubmissions");

            migrationBuilder.DropColumn(
                name: "FileURL",
                table: "AssessmentSubmissions");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "VideoURL",
                table: "Lessons",
                newName: "FilePath");

            migrationBuilder.RenameColumn(
                name: "LessonContent",
                table: "Lessons",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "EnrollStatus",
                table: "Enrollments",
                newName: "PaymentStatus");

            migrationBuilder.RenameColumn(
                name: "Score",
                table: "AssessmentSubmissions",
                newName: "Grade");

            migrationBuilder.AddColumn<string>(
                name: "ClassName",
                table: "Students",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EnrollmentDate",
                table: "Students",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CourseId1",
                table: "Enrollments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "Enrollments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeacherId1",
                table: "Courses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileType",
                table: "CourseFiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TeacherId1",
                table: "CourseFiles",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AssessmentId",
                table: "AssessmentSubmissions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "LessonId",
                table: "AssessmentSubmissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AssessmentQuestions",
                columns: table => new
                {
                    QuestionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssessmentId = table.Column<int>(type: "int", nullable: false),
                    QuestionDetail = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AnswerA = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnswerB = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnswerC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnswerD = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectAnswer = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentQuestions", x => x.QuestionId);
                    table.ForeignKey(
                        name: "FK_AssessmentQuestions_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "AssessmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_UserId",
                table: "Teachers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_UserId",
                table: "Students",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_CourseId",
                table: "Lessons",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CourseId",
                table: "Enrollments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CourseId1",
                table: "Enrollments",
                column: "CourseId1");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentId",
                table: "Enrollments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CategoryId",
                table: "Courses",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_TeacherId",
                table: "Courses",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_TeacherId1",
                table: "Courses",
                column: "TeacherId1");

            migrationBuilder.CreateIndex(
                name: "IX_CourseFiles_CourseId",
                table: "CourseFiles",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseFiles_TeacherId",
                table: "CourseFiles",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseFiles_TeacherId1",
                table: "CourseFiles",
                column: "TeacherId1");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CourseId",
                table: "Certificates",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_StudentId",
                table: "Certificates",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentSubmissions_AssessmentId",
                table: "AssessmentSubmissions",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentSubmissions_LessonId",
                table: "AssessmentSubmissions",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentSubmissions_StudentId",
                table: "AssessmentSubmissions",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_CourseId",
                table: "Assessments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Admins_UserId",
                table: "Admins",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentQuestions_AssessmentId",
                table: "AssessmentQuestions",
                column: "AssessmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_Users_UserId",
                table: "Admins",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_Courses_CourseId",
                table: "Assessments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentSubmissions_Assessments_AssessmentId",
                table: "AssessmentSubmissions",
                column: "AssessmentId",
                principalTable: "Assessments",
                principalColumn: "AssessmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentSubmissions_Lessons_LessonId",
                table: "AssessmentSubmissions",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "LessonId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentSubmissions_Students_StudentId",
                table: "AssessmentSubmissions",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Courses_CourseId",
                table: "Certificates",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Students_StudentId",
                table: "Certificates",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseFiles_Courses_CourseId",
                table: "CourseFiles",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Categories_CategoryId",
                table: "Courses",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Teachers_TeacherId",
                table: "Courses",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "TeacherId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Teachers_TeacherId1",
                table: "Courses",
                column: "TeacherId1",
                principalTable: "Teachers",
                principalColumn: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Courses_CourseId",
                table: "Enrollments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Courses_CourseId1",
                table: "Enrollments",
                column: "CourseId1",
                principalTable: "Courses",
                principalColumn: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Students_StudentId",
                table: "Enrollments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Courses_CourseId",
                table: "Lessons",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Users_UserId",
                table: "Students",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_Users_UserId",
                table: "Teachers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admins_Users_UserId",
                table: "Admins");

            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_Courses_CourseId",
                table: "Assessments");

            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentSubmissions_Assessments_AssessmentId",
                table: "AssessmentSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentSubmissions_Lessons_LessonId",
                table: "AssessmentSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentSubmissions_Students_StudentId",
                table: "AssessmentSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Courses_CourseId",
                table: "Certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Students_StudentId",
                table: "Certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseFiles_Courses_CourseId",
                table: "CourseFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseFiles_Teachers_TeacherId",
                table: "CourseFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseFiles_Teachers_TeacherId1",
                table: "CourseFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Categories_CategoryId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Teachers_TeacherId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Teachers_TeacherId1",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Courses_CourseId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Courses_CourseId1",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Students_StudentId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Courses_CourseId",
                table: "Lessons");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Users_UserId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_Users_UserId",
                table: "Teachers");

            migrationBuilder.DropTable(
                name: "AssessmentQuestions");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_UserId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Students_UserId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_CourseId",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_CourseId",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_CourseId1",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_StudentId",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Courses_CategoryId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_TeacherId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_TeacherId1",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_CourseFiles_CourseId",
                table: "CourseFiles");

            migrationBuilder.DropIndex(
                name: "IX_CourseFiles_TeacherId",
                table: "CourseFiles");

            migrationBuilder.DropIndex(
                name: "IX_CourseFiles_TeacherId1",
                table: "CourseFiles");

            migrationBuilder.DropIndex(
                name: "IX_Certificates_CourseId",
                table: "Certificates");

            migrationBuilder.DropIndex(
                name: "IX_Certificates_StudentId",
                table: "Certificates");

            migrationBuilder.DropIndex(
                name: "IX_AssessmentSubmissions_AssessmentId",
                table: "AssessmentSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_AssessmentSubmissions_LessonId",
                table: "AssessmentSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_AssessmentSubmissions_StudentId",
                table: "AssessmentSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_Assessments_CourseId",
                table: "Assessments");

            migrationBuilder.DropIndex(
                name: "IX_Admins_UserId",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "ClassName",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "EnrollmentDate",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CourseId1",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "TeacherId1",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "FileType",
                table: "CourseFiles");

            migrationBuilder.DropColumn(
                name: "TeacherId1",
                table: "CourseFiles");

            migrationBuilder.DropColumn(
                name: "LessonId",
                table: "AssessmentSubmissions");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Lessons",
                newName: "VideoURL");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Lessons",
                newName: "LessonContent");

            migrationBuilder.RenameColumn(
                name: "PaymentStatus",
                table: "Enrollments",
                newName: "EnrollStatus");

            migrationBuilder.RenameColumn(
                name: "Grade",
                table: "AssessmentSubmissions",
                newName: "Score");

            migrationBuilder.AddColumn<string>(
                name: "PDFFilePath",
                table: "Lessons",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "isAproved",
                table: "Courses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Certificates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "AssessmentId",
                table: "AssessmentSubmissions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Feedback",
                table: "AssessmentSubmissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileURL",
                table: "AssessmentSubmissions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
