using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;

namespace RSD_E_Learning.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentFinalExamController : Controller
    {
        private readonly DB _db;

        public StudentFinalExamController(DB db)
        {
            _db = db;
        }

        // ================== LIST FINAL EXAMS ==================
        public async Task<IActionResult> Index(int courseId)
        {
            var finals = await _db.FinalExams
                .Where(f => f.CourseId == courseId)
                .OrderByDescending(f => f.FinalId)
                .ToListAsync();

            ViewBag.CourseId = courseId;
            return View(finals);
        }

        // ================== ATTEMPT FINAL ==================
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Attempt(int finalId)
        {
            var userEmail = User.Identity!.Name;

            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User!.Email == userEmail);

            if (student == null)
                return Unauthorized();

            //  ATTEMPT LIMIT CHECK (NEW)
            int attemptCount = await _db.FinalAttempts.CountAsync(a =>
                a.StudentId == student.StudentId &&
                a.FinalId == finalId);

            if (attemptCount >= 3)
            {
                TempData["Error"] = "You have reached the maximum number of attempts (3) for this final exam.";
                return RedirectToAction("ResultLimit");
            }

            var final = await _db.FinalExams
                .Include(f => f.FinalQuestions)
                .FirstOrDefaultAsync(f => f.FinalId == finalId);

            if (final == null)
                return NotFound();

            return View(final);
        }

        [Authorize(Roles = "Student")]
        public IActionResult ResultLimit()
        {
            return View();
        }



        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Submit(
        int FinalId,
        Dictionary<int, string> answers)
        {
            var userEmail = User.Identity!.Name;

            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User!.Email == userEmail);

            if (student == null)
                return Unauthorized();

            var final = await _db.FinalExams
                .Include(f => f.FinalQuestions)
                .FirstOrDefaultAsync(f => f.FinalId == FinalId);

            if (final == null)
                return NotFound();

            int correct = 0;

            var attempt = new DB.FinalAttempt
            {
                StudentId = student.StudentId,
                FinalId = FinalId
            };

            _db.FinalAttempts.Add(attempt);
            await _db.SaveChangesAsync(); // get AttemptId

            foreach (var q in final.FinalQuestions)
            {
                if (!answers.ContainsKey(q.QuestionId))
                    continue;

                bool isCorrect = answers[q.QuestionId] == q.CorrectAnswer;

                if (isCorrect)
                    correct++;

                _db.StudentFinalAnswers.Add(new DB.StudentFinalAnswer
                {
                    AttemptId = attempt.AttemptId,
                    QuestionId = q.QuestionId,
                    SelectedAnswer = answers[q.QuestionId],
                    IsCorrect = isCorrect
                });
            }

            int totalQuestions = final.FinalQuestions.Count;
            double scorePercent = (double)correct / totalQuestions * 100;

            attempt.Score = scorePercent;
            attempt.IsPassed = scorePercent >= final.PassingMark;

            await _db.SaveChangesAsync();

            //  IF PASSED → UNLOCK COURSE
            if (attempt.IsPassed)
            {
                var progress = await _db.StudentCourseProgresses
                    .FirstOrDefaultAsync(p =>
                        p.StudentId == student.StudentId &&
                        p.CourseId == final.CourseId);

                if (progress != null)
                {
                    progress.ProgressPercentage = 100;
                    progress.UpdatedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }
            }

            TempData["Result"] = attempt.IsPassed
                ? "Congratulations! You passed the final exam."
                : "You did not meet the passing mark.";

            return RedirectToAction("Result", new { attemptId = attempt.AttemptId });
        }
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Result(int attemptId)
        {
            var attempt = await _db.FinalAttempts
                .Include(a => a.FinalExam)
                .FirstOrDefaultAsync(a => a.AttemptId == attemptId);

            if (attempt == null)
                return NotFound();

            ViewBag.Score = attempt.Score;
            ViewBag.Passed = attempt.IsPassed;
            ViewBag.PassingMark = attempt.FinalExam!.PassingMark;

            // 
            ViewBag.FinalId = attempt.FinalId;
            ViewBag.CourseId = attempt.FinalExam.CourseId;

            return View();
        }



    }
}
