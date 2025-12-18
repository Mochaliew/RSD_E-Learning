using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RSD_E_Learning.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        // STUDENT DASHBOARD
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}

