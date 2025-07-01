using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VanQuangTin_2280603267_Lab04.Areas.Admin.Models;
using VanQuangTin_2280603267_Lab04.Models;

namespace VanQuangTin_2280603267_Lab04.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class Admin_ManageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public Admin_ManageController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Chỉ cần trả về View, không cần làm gì thêm
            return View();
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            // Tổng số đơn
            ViewBag.TotalOrders = _context.Orders.Count();

            // Tổng doanh thu
            ViewBag.TotalRevenue = _context.Orders.Sum(o => o.TotalPrice);

            // Tổng số người dùng
            ViewBag.TotalUsers = _context.Users.Count();

            // Thống kê theo danh mục
            var categoryStats = _context.OrderDetails
                .GroupBy(d => d.Product.Category.Name) // Giả sử mỗi Product có navigation đến Category
                .Select(g => new
                {
                    CategoryName = g.Key,
                    QuantitySold = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Quantity * x.Price)
                }).OrderByDescending(g => g.QuantitySold).ToList();

            ViewBag.CategoryStats = JsonConvert.SerializeObject(categoryStats);



            // Đơn hàng theo trạng thái
            var orderStatus = _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                }).ToList();

            ViewBag.OrderStatus = JsonConvert.SerializeObject(orderStatus);

            return View();
        }
    }

}
