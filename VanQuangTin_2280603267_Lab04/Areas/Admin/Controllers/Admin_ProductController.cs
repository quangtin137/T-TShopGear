using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using VanQuangTin_2280603267_Lab04.Areas.Admin.Models;
using VanQuangTin_2280603267_Lab04.Repositories;
using VanQuangTin_2280603267_Lab04.Models;

namespace VanQuangTin_2280603267_Lab04.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class Admin_ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public Admin_ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public IActionResult Index(string sortOrder, List<string> priceRange, int page = 1, int pageSize = 12)
        {
            var products = _productRepository.GetAll(); // IQueryable<Product>

            // ✅ Lọc nhiều khoảng giá
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

                // Tránh lỗi EF không thể xử lý nhiều điều kiện OR động
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

            // ✅ Phân trang
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

        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile imageUrl)
        {
            if (imageUrl == null || imageUrl.Length == 0)
            {
                ModelState.AddModelError("ImageUrl", "Vui lòng chọn hình ảnh.");
            }

            if (ModelState.IsValid)
            {
                product.ImageUrl = await SaveImage(imageUrl);
                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }

        public async Task<string> SaveImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                throw new ArgumentException("File ảnh không hợp lệ.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return "/images/" + uniqueFileName;
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product, IFormFile imageUrl)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryRepository.GetAllAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
                return View(product);
            }

            var existingProduct = await _productRepository.GetByIdAsync(product.Id);
            if (existingProduct == null) return NotFound();

            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Description = product.Description;
            existingProduct.CategoryId = product.CategoryId;

            if (imageUrl != null && imageUrl.Length > 0)
            {
                existingProduct.ImageUrl = await SaveImage(imageUrl);
            }

            await _productRepository.UpdateAsync(existingProduct);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product == null ? NotFound() : View(product);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product == null ? NotFound() : View(product);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                ViewBag.Keyword = "";
                return View(new List<Product>());
            }

            var results = await _productRepository.SearchByNameAsync(keyword);
            ViewBag.Keyword = keyword;
            return View(results);
        }
    }
}
