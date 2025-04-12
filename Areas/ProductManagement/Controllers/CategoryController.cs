using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventoryManagementSystem.Areas.ProductManagement.Models;
using SmartInventoryManagementSystem.Data;
using SmartInventoryManagementSystem.Models;

namespace SmartInventoryManagementSystem.Areas.ProductManagement.Controllers
{
    [Area("ProductManagement")]
    [Route("[area]/[controller]")]
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ApplicationDbContext context, ILogger<CategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("CategoryController Index visited at {Time}", DateTime.Now);
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        [HttpGet("Add")]
        [Authorize(Roles = "Admin")]
        public IActionResult Add()
        {
            _logger.LogInformation("CategoryController Add (GET) visited at {Time}", DateTime.Now);
            return View();
        }

        [HttpPost("Add")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                _logger.LogWarning("Attempted to add category with empty name");
                ModelState.AddModelError("Name", "Category name is required.");
                return View(category);
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Category added: {@Category}", category);
            return RedirectToAction("Index");
        }

        [HttpGet("Update/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {id} not found", id);
                return NotFound();
            }
            return View(category);
        }

        [HttpPost("Update/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(category.Name))
            {
                ModelState.AddModelError("Name", "Category name is required.");
                return View(category);
            }

            try
            {
                _context.Categories.Update(category);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Category with ID {id} updated successfully", id);
                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogError("Concurrency error while updating category ID {id}", id);
                return NotFound();
            }
        }

        [HttpGet("Delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {id} not found for delete", id);
                return NotFound();
            }

            return View(category);
        }

        [HttpPost("Delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Category with ID {id} deleted", id);
            }
            return RedirectToAction("Index");
        }
    }
}