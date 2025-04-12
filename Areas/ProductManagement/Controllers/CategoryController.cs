using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventoryManagementSystem.Areas.ProductManagement.Models;
using SmartInventoryManagementSystem.Data;

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
            try
            {
                var categories = await _context.Categories.ToListAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category list at {Time}", DateTime.Now);
                return View("Error");
            }
        }

        [HttpGet("Add")]
        [Authorize(Roles = "Admin")]
        public IActionResult Add()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading add category form at {Time}", DateTime.Now);
                return View("Error");
            }
        }

        [HttpPost("Add")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add(Category category)
        {
            try
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
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while adding category at {Time}", DateTime.Now);
                ModelState.AddModelError("", "There was a problem saving the category due to a database error. Please try again later.");
                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while adding category at {Time}", DateTime.Now);
                ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
                return View(category);
            }
        }

        [HttpGet("Update/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    _logger.LogWarning("Category with ID {id} not found", id);
                    return NotFound();
                }
                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category update form for ID {id} at {Time}", id, DateTime.Now);
                return View("Error");
            }
        }

        [HttpPost("Update/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, Category category)
        {
            // _logger.LogInformation("CategoryController Add (POST) visited at {Time}", DateTime.Now);
            
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
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    _logger.LogWarning("Category with ID {id} not found for delete", id);
                    return NotFound();
                }
                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category delete view for ID {id} at {Time}", id, DateTime.Now);
                return View("Error");
            }
        }

        [HttpPost("Delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                _logger.LogInformation("DeleteConfirmed called for Category ID {id}", id);
                var category = await _context.Categories.FindAsync(id);
                if (category != null)
                {
                    _context.Categories.Remove(category);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Category with ID {id} deleted", id);
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category with ID {id} at {Time}", id, DateTime.Now);
                return View("Error");
            }
        }
    }
}