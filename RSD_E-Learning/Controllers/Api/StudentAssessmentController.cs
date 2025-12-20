using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;

namespace RSD_E_Learning.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentAssessmentController : Controller
    {
        private readonly DB _db;

        public StudentAssessmentController(DB db)
        {
            _db = db;
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Details(int id)
        {
            var email = User.Identity!.Name;

            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User!.Email == email);

            if (student == null)
                return Unauthorized();

            var assessment = await _db.Assessments
                .Include(a => a.Course)
                .Include(a => a.Questions) // ⭐ ALWAYS include questions
                .FirstOrDefaultAsync(a => a.AssessmentId == id);

            if (assessment == null)
                return NotFound();

            // Enrollment check
            var isEnrolled = await _db.Enrollments.AnyAsync(e =>
                e.StudentId == student.StudentId &&
                e.CourseId == assessment.CourseId);

            if (!isEnrolled)
                return Forbid();

            // ⭐ CHECK IF STUDENT HAS ALREADY ATTEMPTED
            var hasAttempted = await _db.AssessmentAttempts.AnyAsync(a =>
                a.StudentId == student.StudentId &&
                a.AssessmentId == assessment.AssessmentId);

            ViewBag.HasAttempted = hasAttempted;


            return View(assessment);
        }


        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Submit(int assessmentId)
        {
            var email = User.Identity!.Name;

            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User!.Email == email);

            if (student == null)
                return Unauthorized();

            var assessment = await _db.Assessments
                .Include(a => a.Questions)
                .FirstOrDefaultAsync(a => a.AssessmentId == assessmentId);

            if (assessment == null)
                return NotFound();

            int totalQuestions = assessment.Questions.Count;
            int correctCount = 0;

            var attempt = new DB.AssessmentAttempt
            {
                StudentId = student.StudentId,
                AssessmentId = assessment.AssessmentId,
                AttemptedAt = DateTime.UtcNow
            };

            _db.AssessmentAttempts.Add(attempt);
            await _db.SaveChangesAsync();

            foreach (var q in assessment.Questions)
            {
                var selected = Request.Form[$"answer_{q.QuestionId}"];
                if (string.IsNullOrEmpty(selected)) continue;

                bool isCorrect = selected == q.CorrectAnswer;
                if (isCorrect) correctCount++;

                _db.StudentAnswers.Add(new DB.StudentAnswer
                {
                    AttemptId = attempt.AttemptId,
                    QuestionId = q.QuestionId,
                    SelectedAnswer = selected!,
                    IsCorrect = isCorrect
                });
            }

            double scorePercentage =
                totalQuestions == 0 ? 0 :
                (double)correctCount / totalQuestions * 100;

            attempt.Score = scorePercentage;
            attempt.IsPassed = scorePercentage >= assessment.PassingMark;

            await _db.SaveChangesAsync();

            // ✅ PASS EVERYTHING RESULT NEEDS
            ViewBag.Total = totalQuestions;
            ViewBag.Correct = correctCount;
            ViewBag.Score = Math.Round(scorePercentage, 2);
            ViewBag.PassingMark = assessment.PassingMark;
            ViewBag.IsPassed = attempt.IsPassed;
            ViewBag.CourseId = assessment.CourseId; // 🔥 THIS FIXES EVERYTHING

            return View("Result");
        }







    }
}
