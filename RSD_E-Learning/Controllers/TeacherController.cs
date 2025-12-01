using Microsoft.AspNetCore.Mvc;

namespace RSD_E_Learning.Controllers
{
    public class TeacherController : Controller
    {
        public IActionResult TeacherIndex()
        {
            return View();
        }

        public IActionResult CreateCourse()
        {
            return View();
        }

        public IActionResult ViewCourse()
        {
            return View();
        }

        public IActionResult CreateAssessment()
        {
            return View();
        }

        public IActionResult ViewAssessment()
        {
            return View();
        }

        public IActionResult TeacherDashboard()
        {
            return View();
        }
    }
}