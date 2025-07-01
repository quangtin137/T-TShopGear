using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VanQuangTin_2280603267_Lab04.Models;
using VanQuangTin_2280603267_Lab04.Repositories;

namespace VanQuangTin_2280603267_Lab04.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _productRepository;

        public HomeController(IProductRepository productRepository, ILogger<HomeController> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        public IActionResult Index(string sortOrder, List<string> priceRange, int page = 1, int pageSize = 12)
        {
            var products = _productRepository.GetAll(); // IQueryable<Product>

            // ✅ Lọc theo nhiều khoảng giá
            if (priceRange != null && priceRange.Any() && !priceRange.Contains("all"))
            {
                var filteredList = new List<Product>();

                foreach (var rangeStr in priceRange)
                {
                    var parts = rangeStr.Split('-');
                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], out int min) &&
                        int.TryParse(parts[1], out int max))
                    {
                        var matched = products.Where(p => p.Price * 1000 >= min && p.Price * 1000 <= max);
                        filteredList.AddRange(matched);
                    }
                }

                // ✅ Bắt buộc phải chuyển sang in-memory để tránh lỗi LINQ to SQL
                products = filteredList.Distinct().ToList().AsQueryable();
            }

            // ✅ Sắp xếp
            products = sortOrder switch
            {
                "price_asc" => products.OrderBy(p => p.Price),
                "price_desc" => products.OrderByDescending(p => p.Price),
                "name_asc" => products.OrderBy(p => p.Name),
                "name_desc" => products.OrderByDescending(p => p.Name),
                _ => products
            };

            // ✅ Tổng số sản phẩm & phân trang
            int totalItems = products.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.TotalPages = totalPages > 0 ? totalPages : 1;

            var pagedProducts = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // ✅ Truyền ViewBag cho Razor View
            ViewBag.CurrentPage = page;
            ViewBag.SortOrder = sortOrder;
            ViewBag.PriceRange = priceRange;

            return View(pagedProducts);
        }

        [HttpPost]
        public IActionResult Filter(List<string> priceRange, string sortOrder)
        {
            return RedirectToAction("Index", new { sortOrder = sortOrder, priceRange = priceRange });
        }

        [HttpPost]
        public IActionResult Sort(string sortOrder, List<string> priceRange)
        {
            return RedirectToAction("Index", new { sortOrder = sortOrder, priceRange = priceRange });
        }


        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string message)
        {
            ViewBag.ErrorMessage = message;
            return View();
        }
    }
}
