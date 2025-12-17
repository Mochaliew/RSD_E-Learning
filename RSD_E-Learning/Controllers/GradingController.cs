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
            // 1️⃣ Get all questions for the assessment
            var questions = _context.AssessmentQuestions
                .Where(q => q.AssessmentId == model.AssessmentId)
                .ToList();

            if (!questions.Any())
                return BadRequest("Assessment not found.");

            // 2️⃣ Create attempt
            var attempt = new AssessmentAttempt
            {
                StudentId = model.StudentId,
                AssessmentId = model.AssessmentId,
                AttemptedAt = DateTime.UtcNow
            };

            _context.AssessmentAttempts.Add(attempt);
            _context.SaveChanges();

            int correctCount = 0;

            // 3️⃣ Grade each answer
            foreach (var answer in model.Answers)
            {
                var question = questions.FirstOrDefault(q => q.QuestionId == answer.QuestionId);
                if (question == null) continue;

                bool isCorrect = question.CorrectAnswer == answer.SelectedAnswer;
                if (isCorrect) correctCount++;

                _context.StudentAnswers.Add(new StudentAnswer
                {
                    AttemptId = attempt.AttemptId,
                    QuestionId = question.QuestionId,
                    SelectedAnswer = answer.SelectedAnswer,
                    IsCorrect = isCorrect
                });
            }

            // 4️⃣ Calculate score
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
