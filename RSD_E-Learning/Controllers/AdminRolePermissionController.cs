using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RSD_E_Learning.Models;

[Authorize(Roles = "Admin")]
public class AdminRolePermissionController : Controller
{
    public IActionResult Index()
    {
        var roles = new List<RolePermissionVm>
        {
            new RolePermissionVm
            {
                RoleName = "Admin",
                Permissions = new()
                {
                    "Manage system settings",
                    "Approve or reject courses",
                    "Manage teachers and students",
                    "View reports and statistics"
                }
            },
            new RolePermissionVm
            {
                RoleName = "Teacher",
                Permissions = new()
                {
                    "Create courses",
                    "Upload learning materials",
                    "Create assessments"
                }
            },
            new RolePermissionVm
            {
                RoleName = "Student",
                Permissions = new()
                {
                    "Enroll in courses",
                    "Access learning materials",
                    "Download certificates"
                }
            }
        };

        return View(roles);
    }
}
