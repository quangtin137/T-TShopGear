using GearShop.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VanQuangTin_2280603267_Lab04.Models;

namespace GearShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class Admin_UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public Admin_UserController(UserManager<ApplicationUser> userManager,
                                    RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Danh sách người dùng
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();

            // Tạo dictionary: userId → role
            var userRoles = new Dictionary<string, string>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? "Chưa có";
            }

            ViewBag.UserRoles = userRoles;

            return View(users);
        }

        // GET: Thêm user
        public IActionResult Create()
        {
            return View();
        }

        // POST: Thêm user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser model, string password)
        {
            if (ModelState.IsValid)
            {
                var result = await _userManager.CreateAsync(model, password);
                if (result.Succeeded)
                {
                    // Gán role mặc định nếu cần, ví dụ: "User"
                    await _userManager.AddToRoleAsync(model, "User");
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }
    }
}
