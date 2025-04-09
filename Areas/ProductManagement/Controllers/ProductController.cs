using SmartInventoryManagementSystem.Areas.ProductManagement.Models;
using SmartInventoryManagementSystem.Data;
using SmartInventoryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SmartInventoryManagementSystem.Areas.ProductManagement.Controllers
{
    [Area("ProductManagement")]
    [Route("[area]/[controller]/[action]")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public IActionResult Index()
        {
            var products = _context.Products.Include(p => p.Category).ToList();

            // count low-stock products for the alert
            int lowStockCount = products.Count(p => p.Quantity < 10);
            ViewBag.LowStockCount = lowStockCount;

            return View(products);
        }
        
        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        // save new product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Product product)
        {
            var cat = _context.Categories.Find(product.CategoryId);
            
            if (cat == null)
            {
                return RedirectToAction("Index");
            }
            {
                product.Category = cat;
            }
            
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                ViewBag.Categories = _context.Categories.ToList();
                return View(product);
            }
            
            _context.Products.Add(product);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        
        [HttpGet]
        public IActionResult Update(int id)
        {
            var product = _context.Products.Find(id);
            
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int id, [Bind("ProductId, Name, CategoryId, Price, Quantity, LowStockThreshold")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            // fetch the existing product from the database
            var existingProduct = _context.Products.Find(id);
            
            if (existingProduct == null)
            {
                return NotFound();
            }

            // assign updated values
            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Quantity = product.Quantity;
            existingProduct.LowStockThreshold = product.LowStockThreshold;

            // ensure the category exists and update it
            var category = _context.Categories.Find(product.CategoryId);
            if (category == null)
            {
                ModelState.AddModelError("CategoryId", "Invalid category selected.");
                ViewBag.Categories = _context.Categories.ToList();
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

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.ProductId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction("Index");
        }
        
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int productId)
        {
            var product = _context.Products.Find(productId);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return NotFound();
        }

        // helper method: check if product exists
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
        
        [HttpGet]
        public IActionResult Summary()
        {
            var totalStock = _context.Products.Sum(p => p.Quantity);
            var lowStockCount = _context.Products.Count(p => p.Quantity < 10);

            var categorySummary = _context.Categories
                .Select(c => new
                {
                    CategoryName = c.Name,
                    ProductCount = _context.Products.Count(p => p.CategoryId == c.CategoryId)
                })
                .ToList();

            ViewBag.TotalStock = totalStock;
            ViewBag.LowStockCount = lowStockCount;
            ViewBag.CategorySummary = categorySummary;

            return View();
        }
        
        // product search (with search, filtering, and sorting)
        [HttpGet]
        public IActionResult Search(string searchString, int? categoryId, bool? lowStockOnly, float? minPrice, float? maxPrice, string sortOrder)
        {
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            // search by name
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString));
            }

            // filter by category
            if (categoryId.HasValue && categoryId > 0)
            {
                products = products.Where(p => p.Category.CategoryId == categoryId.Value);
            }

            // filter by low stock
            if (lowStockOnly.HasValue && lowStockOnly.Value)
            {
                products = products.Where(p => p.Quantity < p.LowStockThreshold);
            }

            // filter by price range
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value);
            }

            // sorting logic
            switch (sortOrder)
            {
                case "name_asc":
                    products = products.OrderBy(p => p.Name);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "quantity_asc":
                    products = products.OrderBy(p => p.Quantity);
                    break;
                case "quantity_desc":
                    products = products.OrderByDescending(p => p.Quantity);
                    break;
                default:
                    products = products.OrderBy(p => p.ProductId);
                    break;
            }

            ViewBag.Categories = _context.Categories.ToList();
            return View(products.ToList());
        }
    }
}