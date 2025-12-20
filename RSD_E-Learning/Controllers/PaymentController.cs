using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;
using Stripe.Checkout;
using static RSD_E_Learning.Models.DB;

[Authorize(Roles = "Student")]
public class PaymentController : Controller
{
    private readonly DB _db;

    public PaymentController(DB db)
    {
        _db = db;
    }

    public async Task<IActionResult> Checkout(int courseId)
    {
        var course = await _db.Courses.FindAsync(courseId);
        if (course == null) return NotFound();

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "myr",
                        UnitAmount = 5000, // RM50 (example)
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = course.Title
                        }
                    },
                    Quantity = 1
                }
            },
            Mode = "payment",
            SuccessUrl = Url.Action(
                "Success", "Payment",
                new { courseId },
                Request.Scheme),
            CancelUrl = Url.Action(
                "Cancel", "Payment",
                null,
                Request.Scheme)
        };

        var service = new SessionService();
        var session = service.Create(options);

        return Redirect(session.Url);
    }

    public async Task<IActionResult> Success(int courseId)
    {
        var email = User.Identity!.Name;

        var student = await _db.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.User!.Email == email);

        if (student == null) return Unauthorized();

        var enrolled = await _db.Enrollments.AnyAsync(e =>
            e.StudentId == student.StudentId &&
            e.CourseId == courseId);

        if (!enrolled)
        {
            _db.Enrollments.Add(new Enrollment
            {
                StudentId = student.StudentId,
                CourseId = courseId,
                PaymentStatus = true,
                PaymentMethod = "Stripe",
                AmountPaid = 50
            });

            _db.StudentCourseProgresses.Add(new StudentCourseProgress
            {
                StudentId = student.StudentId,
                CourseId = courseId
            });

            await _db.SaveChangesAsync();
        }

        return RedirectToAction("MyCourses", "StudentCourse");
    }

    public IActionResult Cancel()
    {
        TempData["Error"] = "Payment cancelled.";
        return RedirectToAction("Index", "StudentCourse");
    }

}
