using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Drawing.Printing;
using VanQuangTin_2280603267_Lab04.Models;
using VanQuangTin_2280603267_Lab04.Repositories;

namespace VanQuangTin_2280603267_Lab04.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public IActionResult Index(string sortOrder, string priceRange, int page = 1, int pageSize = 12)
        {
            var products = _productRepository.GetAll(); // IQueryable

            // Lọc theo khoảng giá
            if (!string.IsNullOrEmpty(priceRange) && priceRange != "all")
            {
                var range = priceRange.Split('-');
                if (range.Length == 2 &&
                    int.TryParse(range[0], out int minPrice) &&
                    int.TryParse(range[1], out int maxPrice))
                {
                    products = products.Where(p => p.Price * 1000 >= minPrice && p.Price * 1000 <= maxPrice);
                }
            }

            // Sắp xếp theo giá
            products = sortOrder switch
            {
                "asc" => products.OrderBy(p => p.Price),
                "desc" => products.OrderByDescending(p => p.Price),
                _ => products
            };

            // Tổng số sản phẩm
            int totalItems = products.Count();

            // Tính số trang và đảm bảo luôn ≥ 1
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.TotalPages = totalPages > 0 ? totalPages : 1;

            // Phân trang
            var pagedProducts = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.SortOrder = sortOrder;
            ViewBag.PriceRange = priceRange;

            return View(pagedProducts);
        }




        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View();
        }

        public async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile imageUrl)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    product.ImageUrl = await SaveImage(imageUrl);
                }
                await _productRepository.AddAsync(product);

                return RedirectToAction(nameof(Index));
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View(product);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product, IFormFile imageUrl)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    product.ImageUrl = await SaveImage(imageUrl);
                }
                await _productRepository.UpdateAsync(product);

                return RedirectToAction(nameof(Index));
            }
            
            return View(product);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if(product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View();
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
