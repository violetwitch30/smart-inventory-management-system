using Microsoft.AspNetCore.Authorization;
using SmartInventoryManagementSystem.Areas.ProductManagement.Models;
using SmartInventoryManagementSystem.Data;
using SmartInventoryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SmartInventoryManagementSystem.Areas.ProductManagement.Controllers
{
    [Area("ProductManagement")]
    [Route("[area]/[controller]")]
    //[Authorize]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ApplicationDbContext context, ILogger<ProductController> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("ProductController Index visited at {Time}", DateTime.Now);
            
            var products = await _context.Products.Include(p => p.Category).ToListAsync();

            // count low-stock products for the alert
            int lowStockCount = products.Count(p => p.Quantity < 10);
            ViewBag.LowStockCount = lowStockCount;

            return View(products);
        }
        
        [HttpGet("Add")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add()
        {
            _logger.LogInformation("ProductController Add (GET) visited at {Time}", DateTime.Now);
            
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View();
        }

        // save new product
        [HttpPost("Add")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add(Product product)
        {
            _logger.LogInformation("ProductController Add (POST) visited at {Time}", DateTime.UtcNow);

            // Validate category existence
            var cat = await _context.Categories.FindAsync(product.CategoryId);
            if (cat == null)
            {
                _logger.LogWarning("Invalid category ID {CategoryId} submitted", product.CategoryId);
                return RedirectToAction("Index");
            }
            {
                product.Category = cat;
            }
            
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                _logger.LogWarning("Model validation failed or product name is empty");
                ViewBag.Categories = await _context.Categories.ToListAsync();
                return View(product);
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Product created: {@Product}", product);
            return RedirectToAction("Index");
        }
        
        [HttpGet("Update/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id)
        {
            _logger.LogInformation("ProductController Update visited at {Time}", DateTime.Now);
            
            var product = await _context.Products.FindAsync(id);
            
            if (product == null)
            {
                _logger.LogWarning("Product with {id} not found", id);
                return NotFound();
            }

            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        [HttpPost("Update/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id,
                [Bind("ProductId, Name, Description, CategoryId, Price, Quantity, LowStockThreshold")] Product product)
        {
            // Check for route and model ID match.
            if (id != product.ProductId)
            {
                _logger.LogWarning("Update failed: route id {id} doesn't match product id {ProductId}", id, product.ProductId);
                return NotFound();
            }
            
            try 
            {
                var existingProduct = await _context.Products.FindAsync(id);

                if (existingProduct == null)
                {
                    _logger.LogWarning("Product with ID {id} not found", id);
                    return NotFound();
                }

                // Assign updated values
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Quantity = product.Quantity;
                existingProduct.LowStockThreshold = product.LowStockThreshold;

                // Validate category
                var category = await _context.Categories.FindAsync(product.CategoryId);
                if (category == null)
                {
                    ModelState.AddModelError("CategoryId", "Invalid category selected.");
                    ViewBag.Categories = await _context.Categories.ToListAsync();
                    _logger.LogWarning("Update failed: invalid category for product {id}", id);
                    return View(product);
                }
                existingProduct.Category = category;

                // validate name
                if (string.IsNullOrWhiteSpace(product.Name))
                {
                    ModelState.AddModelError("Name", "Product name is required.");
                    ViewBag.Categories = _context.Categories.ToList();
                    return View(product);
                }
                    
                // Save changes
                await _context.SaveChangesAsync(); 
                _logger.LogInformation("Product with ID {id} updated successfully", id);

                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error while updating product with ID {id}", id);
                if (!await ProductExists(product.ProductId))
                {
                    _logger.LogWarning("Product with ID {id} not found during concurrency check", product.ProductId);
                    return NotFound();
                }
                throw;
            }
        }
        
        private async Task<bool> ProductExists(int id)
        {
            return await _context.Products.AnyAsync(e => e.ProductId == id);
        }

        [HttpGet("Delete/{id}")]
        [Authorize(Roles = "Admin")]
        //[Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {id} not found for delete", id);
                return NotFound();
            }

            return View(product);
        }
        
        [HttpPost("Delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        //[Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("DeleteConfirmed called for Product ID {id}", id);
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Product with ID {id} deleted successfully", id);
                return RedirectToAction("Index");
            }
            _logger.LogWarning("DeleteConfirmed failed: Product with ID {id} not found", id);
            return NotFound();
        }
        
        [HttpGet("Summary")]
        public async Task<IActionResult> Summary()
        {
            try
            {
                var totalStock = await _context.Products.SumAsync(p => p.Quantity);
                var lowStockCount = await _context.Products.CountAsync(p => p.Quantity < 10);

                var categorySummary = await _context.Categories
                    .Select(c => new
                    {
                        CategoryName = c.Name,
                        ProductCount = _context.Products.Count(p => p.CategoryId == c.CategoryId)
                    })
                    .ToListAsync();

                ViewBag.TotalStock = totalStock;
                ViewBag.LowStockCount = lowStockCount;
                ViewBag.CategorySummary = categorySummary;

                _logger.LogInformation("Summary data prepared successfully");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating summary data");
                return View("Error");
            }
        }
        
        [HttpGet("Search")]
        public async Task<IActionResult> Search(string searchString, int? categoryId, bool? lowStockOnly, float? minPrice, float? maxPrice, string sortOrder)
        {
            try
            {
                _logger.LogInformation("Search initiated with filters - searchString: '{searchString}', categoryId: {categoryId}, lowStockOnly: {lowStockOnly}, minPrice: {minPrice}, maxPrice: {maxPrice}, sortOrder: '{sortOrder}'",
                    searchString, categoryId, lowStockOnly, minPrice, maxPrice, sortOrder);

                var query = _context.Products.Include(p => p.Category).AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    string lowered = searchString.ToLower();
                    query = query.Where(p => p.Name.ToLower().Contains(lowered));
                }

                if (categoryId.HasValue && categoryId > 0)
                {
                    query = query.Where(p => p.CategoryId == categoryId.Value);
                }

                if (lowStockOnly == true)
                {
                    query = query.Where(p => p.Quantity < p.LowStockThreshold);
                }

                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= maxPrice.Value);
                }

                switch (sortOrder)
                {
                    case "name_asc":
                        query = query.OrderBy(p => p.Name);
                        break;
                    case "name_desc":
                        query = query.OrderByDescending(p => p.Name);
                        break;
                    case "price_asc":
                        query = query.OrderBy(p => p.Price);
                        break;
                    case "price_desc":
                        query = query.OrderByDescending(p => p.Price);
                        break;
                    case "quantity_asc":
                        query = query.OrderBy(p => p.Quantity);
                        break;
                    case "quantity_desc":
                        query = query.OrderByDescending(p => p.Quantity);
                        break;
                    default:
                        query = query.OrderBy(p => p.ProductId);
                        break;
                }

                var products = await query.ToListAsync();

                _logger.LogInformation("Search completed. {Count} product(s) found.", products.Count);

                ViewBag.Categories = await _context.Categories.ToListAsync();
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_ProductListPartial", products);
                }
                return View("Search", products); // Reuses the Index view with filtered results
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching products");
                return View("Error"); // Optional: you can customize the error view
            }
        }
    }
}