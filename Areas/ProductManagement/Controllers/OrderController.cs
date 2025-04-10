using Microsoft.AspNetCore.Mvc;
using SmartInventoryManagementSystem.Data;
using SmartInventoryManagementSystem.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SmartInventoryManagementSystem.Areas.ProductManagement.Models;

namespace SmartInventoryManagementSystem.Areas.ProductManagement.Controllers
{
    [Area("ProductManagement")]
    [Route("[area]/[controller]/[action]")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderController> _logger;

        // Constructor
        public OrderController(ApplicationDbContext context, ILogger<OrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Order/Create (Display the order creation form)
        [HttpGet("")]
        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("OrderController Index visited at {Time}", DateTime.Now);

            ViewBag.Products = await _context.Products.ToListAsync(); // load products
            return View();
        }

        // POST: Order/Create (Handle order creation)
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order, int[] productIds, int[] quantities)
        {
            _logger.LogInformation("Order creation initiated for customer: {CustomerName}", order.CustomerName);

            // Check initial model state and array input
            if (!ModelState.IsValid || productIds == null || quantities == null ||
                productIds.Length != quantities.Length)
            {
                _logger.LogWarning("Model validation failed or mismatched product/quantity arrays");
                ViewBag.Products = await _context.Products.ToListAsync();
                ModelState.AddModelError("", "Invalid order data.");
                return View(order);
            }

            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToListAsync();

            // Stock validation
            for (int i = 0; i < productIds.Length; i++)
            {
                var product = products.FirstOrDefault(p => p.ProductId == productIds[i]);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found", productIds[i]);
                    ModelState.AddModelError("", "Selected product does not exist.");
                    ViewBag.Products = await _context.Products.ToListAsync();
                    return View(order);
                }

                await _context.Entry(product).ReloadAsync(); // ensure current stock

                if (quantities[i] > product.Quantity)
                {
                    _logger.LogWarning("Stock issue: Requested {Requested}, Available {Available} for {ProductName}",
                        quantities[i], product.Quantity, product.Name);
                    ModelState.AddModelError("",
                        $"Not enough stock for {product.Name}. Available: {product.Quantity}, Requested: {quantities[i]}.");
                    ViewBag.Products = await _context.Products.ToListAsync();
                    return View(order);
                }
            }

            // Check for existing order (same customer)
            var existingOrder = await _context.Orders
                .FirstOrDefaultAsync(
                    o => o.CustomerName == order.CustomerName && o.CustomerEmail == order.CustomerEmail);

            if (existingOrder != null)
            {
                var productIdsList = existingOrder.ProductIds?.ToList() ?? new List<int>();
                var quantitiesList = existingOrder.Quantities?.ToList() ?? new List<int>();

                for (int i = 0; i < productIds.Length; i++)
                {
                    var product = products.FirstOrDefault(p => p.ProductId == productIds[i]);
                    if (product != null)
                    {
                        int index = productIdsList.IndexOf(productIds[i]);
                        if (index >= 0)
                        {
                            if (quantities[i] > product.Quantity)
                            {
                                _logger.LogWarning("Not enough stock to merge order for {ProductName}", product.Name);
                                ModelState.AddModelError("", $"Not enough stock for {product.Name}.");
                                ViewBag.Products = await _context.Products.ToListAsync();
                                return View(order);
                            }

                            quantitiesList[index] += quantities[i];
                        }
                        else
                        {
                            productIdsList.Add(productIds[i]);
                            quantitiesList.Add(quantities[i]);
                        }

                        product.Quantity -= quantities[i];
                    }
                }

                existingOrder.ProductIds = productIdsList.ToArray();
                existingOrder.Quantities = quantitiesList.ToArray();
                _context.Orders.Update(existingOrder);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Order for customer {Customer} merged into existing order ID {OrderId}",
                    order.CustomerName, existingOrder.OrderId);
                return RedirectToAction("Summary", new { id = existingOrder.OrderId });
            }
            else
            {
                // New order
                order.OrderId = await _context.Orders.AnyAsync()
                    ? await _context.Orders.MaxAsync(o => o.OrderId) + 1
                    : 1;

                order.ProductIds = productIds;
                order.Quantities = quantities;

                for (int i = 0; i < productIds.Length; i++)
                {
                    var product = products.FirstOrDefault(p => p.ProductId == productIds[i]);
                    if (product != null)
                    {
                        product.Quantity -= quantities[i];
                    }
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation("New order created: ID {OrderId} for customer {Customer}", order.OrderId,
                    order.CustomerName);
                return RedirectToAction("Summary", new { id = order.OrderId });
            }
        }

        // GET: Order/Summary (Display order summary)
        [HttpGet("Summary")]
        public async Task<IActionResult> Summary(int id)
        {
            _logger.LogInformation("Order Summary requested for Order ID {id}", id);

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null)
            {
                _logger.LogWarning("Order with ID {id} not found", id);
                return NotFound();
            }

            ViewBag.Products = await _context.Products.ToListAsync();
            return View(order);
        }

        // GET: Order/Track (Show the order tracking form)
        [HttpGet("Track")]
        public IActionResult Track()
        {
            _logger.LogInformation("Track page accessed");
            return View();
        }

        // POST: Order/Track (Display order summary for the given customer)
        [HttpPost("Track")]
        public async Task<IActionResult> Track(string customerName, string customerEmail)
        {
            _logger.LogInformation("Order tracking submitted for: {Name}, {Email}", customerName, customerEmail);

            if (string.IsNullOrEmpty(customerName) || string.IsNullOrEmpty(customerEmail))
            {
                _logger.LogWarning("Tracking failed: missing name or email");
                ModelState.AddModelError("", "Name and email are required");
                return View();
            }

            var orders = await _context.Orders
                .Where(o => o.CustomerName == customerName && o.CustomerEmail == customerEmail)
                .ToListAsync();

            if (!orders.Any())
            {
                _logger.LogWarning("No orders found for: {Name}, {Email}", customerName, customerEmail);
                ModelState.AddModelError("", "No orders found for the given name and email");
                return View();
            }

            // Display summary for the latest order
            var latestOrder = orders.OrderByDescending(o => o.OrderDate).FirstOrDefault();

            ViewBag.Products = await _context.Products.ToListAsync();
            _logger.LogInformation("Tracking successful for customer {Name}, showing Order ID {OrderId}", customerName,
                latestOrder?.OrderId);

            return View("Summary", latestOrder);
        }
    }
}