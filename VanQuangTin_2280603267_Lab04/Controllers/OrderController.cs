using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanQuangTin_2280603267_Lab04.Models;

namespace VanQuangTin_2280603267_Lab04.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index() //List
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _context.Orders
                                .Where(o => o.UserId == userId)
                                .ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> Cancel(int id) //delete
        {
            var order = await _context.Orders.FindAsync(id);
            var userId = _userManager.GetUserId(User);

            if (order == null || order.UserId != userId || order.Status != OrderStatus.Pending)
                return NotFound();

            order.Status = OrderStatus.Cancelled;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
