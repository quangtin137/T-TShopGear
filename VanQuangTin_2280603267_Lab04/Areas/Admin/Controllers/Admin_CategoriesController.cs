using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VanQuangTin_2280603267_Lab04.Areas.Admin.Models;
using VanQuangTin_2280603267_Lab04.Models;
using VanQuangTin_2280603267_Lab04.Repositories;

namespace VanQuangTin_2280603267_Lab04.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class Admin_CategoriesController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;

        public Admin_CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                await _categoryRepository.AddAsync(category);
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                await _categoryRepository.UpdateAsync(category);
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public async Task<IActionResult> Details(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _categoryRepository.GetByIdWithProductsAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            if (category.Products != null && category.Products.Any())
            {
                ModelState.AddModelError("", "Không thể xóa danh mục vì có sản phẩm liên kết.");
                return View("Delete", category); // Quay lại view Delete và hiển thị thông báo
            }

            await _categoryRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
