using Microsoft.AspNetCore.Mvc;

namespace RSD_E_Learning.Controllers
{
    public class Teacher : Controller
    {
        public IActionResult CreateCourse()
        {
            return View();
        }
    }
}