using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;
using static RSD_E_Learning.Models.DB;

namespace RSD_E_Learning.Controllers
{
    public class GradingController : Controller
    {
        private readonly DB _context;

        public GradingController(DB context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult SubmitAssessment([FromBody] AssessmentSubmissionVM model)
        {
            // 1️⃣ Validate submission
            if (model.Answers == null || !model.Answers.Any())
                return BadRequest("No answers submitted.");

            // 2️⃣ Prevent duplicate attempts
            bool attempted = _context.AssessmentAttempts
                .Any(a => a.StudentId == model.StudentId
                       && a.AssessmentId == model.AssessmentId);

            if (attempted)
                return BadRequest("Assessment already attempted.");

            // 3️⃣ Get all questions for the assessment
            var questions = _context.AssessmentQuestions
                .Where(q => q.AssessmentId == model.AssessmentId)
                .ToList();

            if (!questions.Any())
                return BadRequest("Assessment not found.");

            // 4️⃣ Create assessment attempt
            var attempt = new AssessmentAttempt
            {
                StudentId = model.StudentId,
                AssessmentId = model.AssessmentId,
                AttemptedAt = DateTime.UtcNow
            };

            _context.AssessmentAttempts.Add(attempt);
            _context.SaveChanges();

            int correctCount = 0;

            // 5️⃣ Grade each answer
            foreach (var answer in model.Answers)
            {
                var question = questions.FirstOrDefault(q => q.QuestionId == answer.QuestionId);
                if (question == null) continue;

                bool isCorrect =
                question.CorrectAnswer.Trim().Equals(
                answer.SelectedAnswer.Trim(),
                StringComparison.OrdinalIgnoreCase
                );

                if (isCorrect) correctCount++;

                _context.StudentAnswers.Add(new StudentAnswer
                {
                    AttemptId = attempt.AttemptId,
                    QuestionId = question.QuestionId,
                    SelectedAnswer = answer.SelectedAnswer,
                    IsCorrect = isCorrect
                });
            }

            // 6️⃣ Calculate score and pass/fail
            double score = (double)correctCount / questions.Count * 100;

            attempt.Score = score;
            attempt.IsPassed = score >= 50;

            _context.SaveChanges();

            return Ok(new
            {
                Score = score,
                Passed = attempt.IsPassed
            });
        }
    }
}
