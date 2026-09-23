using BulkyBook.Business.Services.IServices;
using BulkyBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoryAsync();
            return View("Index", categories);
        }
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePOST(Category category)
        {
            if (!string.IsNullOrEmpty(category.Name) &&
                !await _categoryService.IsCategoryNameUniqueAsync(category.Name))
            {
                ModelState.AddModelError("Name", "Category name already exists.");

            }
            if (ModelState.IsValid)
            {

                await _categoryService.CreateCategoryAsync(category);
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View();
        }


        // Update Category

        // GET
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var category = await _categoryService.GetCategoryByIdAsync(id.Value);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Update")]
        public async Task<IActionResult> UpdatePOST(Category category)
        {
            if (!string.IsNullOrEmpty(category.Name) &&
                !await _categoryService.IsCategoryNameUniqueAsync(category.Name, category.Id))
            {
                ModelState.AddModelError("Name", "Category name already exists.");
            }

            if (ModelState.IsValid)
            {
                await _categoryService.UpdateCategoryAsync(category);
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // Delete Category

        // GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var category = await _categoryService.GetCategoryByIdAsync(id.Value);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePOST(int id)

        {
            await _categoryService.DeleteCategoryAsync(id);
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
