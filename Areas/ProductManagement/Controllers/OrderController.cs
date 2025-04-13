using Microsoft.AspNetCore.Mvc;
using SmartInventoryManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SmartInventoryManagementSystem.Areas.ProductManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SmartInventoryManagementSystem.Areas.ProjectManagement.Models;

namespace SmartInventoryManagementSystem.Areas.ProductManagement.Controllers
{
    [Area("ProductManagement")]
    [Route("[area]/[controller]")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, ILogger<OrderController> logger, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet("")]
        public async Task<IActionResult> Create()
        {
            try
            {
                ViewBag.Products = await _context.Products.ToListAsync();

                // Pre-fill for non-admin users
                if (!User.IsInRole("Admin"))
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        var order = new Order
                        {
                            CustomerName = $"{user.FirstName} {user.LastName}",
                            CustomerEmail = user.Email
                        };
                        return View(order);
                    }
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading the order creation form at {Time} for {User}",
                    DateTime.Now, User?.Identity?.Name ?? "Anonymous");
                // Redirect to a generic error view, or provide a user-friendly message as needed.
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order, int[] productIds, int[] quantities)
        {
            try
            {
                _logger.LogInformation("Order creation initiated for customer: {CustomerName} at {Time}",
                    order.CustomerName, DateTime.Now);
                
                if (!User.IsInRole("Admin"))
                {
                    var user = await _userManager.GetUserAsync(User);
                    order.CustomerName = $"{user.FirstName} {user.LastName}";
                    order.CustomerEmail = user.Email;
                }

                // product and quantity inputs
                if (productIds == null || quantities == null || productIds.Length != quantities.Length)
                {
                    _logger.LogWarning("Invalid product/quantity input for customer: {CustomerName}",
                        order.CustomerName);
                    ModelState.AddModelError("", "Invalid order data.");
                    ViewBag.Products = await _context.Products.ToListAsync();
                    return View(order);
                }
                
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.ProductId))
                    .ToListAsync();

                // check stock levels
                for (int i = 0; i < productIds.Length; i++)
                {
                    var product = products.FirstOrDefault(p => p.ProductId == productIds[i]);
                    if (product == null || quantities[i] > product.Quantity)
                    {
                        _logger.LogWarning("Not enough stock for {ProductName}", product?.Name ?? "Unknown");
                        ModelState.AddModelError("", $"Not enough stock for {product?.Name ?? "Unknown"}.");
                        ViewBag.Products = await _context.Products.ToListAsync();
                        return View(order);
                    }
                }

                Order processedOrder;

                // if an order already exists for this customer
                var existingOrder = await _context.Orders.FirstOrDefaultAsync(
                    o => o.CustomerName == order.CustomerName && o.CustomerEmail == order.CustomerEmail);

                if (existingOrder != null)
                {
                    var productIdsList = existingOrder.ProductIds?.ToList() ?? new List<int>();
                    var quantitiesList = existingOrder.Quantities?.ToList() ?? new List<int>();

                    for (int i = 0; i < productIds.Length; i++)
                    {
                        var index = productIdsList.IndexOf(productIds[i]);
                        if (index >= 0)
                            quantitiesList[index] += quantities[i];
                        else
                        {
                            productIdsList.Add(productIds[i]);
                            quantitiesList.Add(quantities[i]);
                        }
                        
                        var product = products.First(p => p.ProductId == productIds[i]);
                        product.Quantity -= quantities[i];
                    }

                    existingOrder.ProductIds = productIdsList.ToArray();
                    existingOrder.Quantities = quantitiesList.ToArray();
                    _context.Orders.Update(existingOrder);
                    await _context.SaveChangesAsync();

                    processedOrder = existingOrder;
                }
                else
                {
                    order.OrderId = await _context.Orders.AnyAsync()
                        ? await _context.Orders.MaxAsync(o => o.OrderId) + 1
                        : 1;
                    order.ProductIds = productIds;
                    order.Quantities = quantities;
                    
                    foreach (var item in productIds.Select((pid, i) => new { pid, qty = quantities[i] }))
                    {
                        var product = products.First(p => p.ProductId == item.pid);
                        product.Quantity -= item.qty;
                    }

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    processedOrder = order;
                }
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_OrderConfirmationPartial", processedOrder);
                }
                else
                {
                    return RedirectToAction("Summary", new { id = processedOrder.OrderId });
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while creating order for {CustomerName} at {Time}",
                    order.CustomerName, DateTime.Now);
                ModelState.AddModelError("",
                    "There was a problem processing your order due to a database error. Please try again later.");
                ViewBag.Products = await _context.Products.ToListAsync();
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An unexpected error occurred while processing the order for {CustomerName} at {Time}",
                    order.CustomerName, DateTime.Now);
                ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
                ViewBag.Products = await _context.Products.ToListAsync();
                return View(order);
            }
        }

        [HttpGet("Summary")]
        public async Task<IActionResult> Summary(int id)
        {
            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id);
                if (order == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found.", id);
                    return NotFound();
                }

                ViewBag.Products = await _context.Products.ToListAsync();
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order summary for Order ID {OrderId} at {Time}", id, DateTime.Now);
                return View("Error");
            }
        }

        [HttpGet("Track")]
        public async Task<IActionResult> Track()
        {
            try
            {
                if (!User.IsInRole("Admin"))
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        var orders = await _context.Orders
                            .Where(o => o.CustomerEmail == user.Email)
                            .OrderByDescending(o => o.OrderDate)
                            .ToListAsync();

                        if (orders.Count == 0)
                        {
                            _logger.LogInformation("No orders found for user {UserEmail}", user.Email);
                            return View("Track");
                        }

                        ViewBag.Products = await _context.Products.ToListAsync();
                        return View("Summary", orders.First());
                    }
                }
                return View(); // For Admin: allow manual entry
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during order tracking at {Time}", DateTime.Now);
                return View("Error");
            }
        }

        [HttpPost("Track")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Track(string customerName, string customerEmail)
        {
            try
            {
                if (string.IsNullOrEmpty(customerName) || string.IsNullOrEmpty(customerEmail))
                {
                    ModelState.AddModelError("", "Name and email are required");
                    return View();
                }

                var orders = await _context.Orders
                    .Where(o => o.CustomerName == customerName && o.CustomerEmail == customerEmail)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                if (!orders.Any())
                {
                    ModelState.AddModelError("", "No orders found");
                    return View();
                }

                ViewBag.Products = await _context.Products.ToListAsync();
                return View("Summary", orders.First());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking order for customer {CustomerName} at {Time}", customerName, DateTime.Now);
                ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
                return View();
            }
        }
    }
}