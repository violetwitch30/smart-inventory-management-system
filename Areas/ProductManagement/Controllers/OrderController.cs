using Microsoft.AspNetCore.Mvc;
using SmartInventoryManagementSystem.Data;
using SmartInventoryManagementSystem.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SmartInventoryManagementSystem.Areas.ProductManagement.Models;
using Microsoft.AspNetCore.Identity;
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
            _logger.LogInformation("OrderController Index visited at {Time}", DateTime.Now);

            ViewBag.Products = await _context.Products.ToListAsync();

            // Pre-fill for regular user
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order, int[] productIds, int[] quantities)
        {
            _logger.LogInformation("Order creation initiated for customer: {CustomerName}", order.CustomerName);

            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                order.CustomerName = $"{user.FirstName} {user.LastName}";
                order.CustomerEmail = user.Email;
            }

            if (productIds == null || quantities == null || productIds.Length != quantities.Length)
            {
                _logger.LogWarning("Invalid product/quantity input");
                ModelState.AddModelError("", "Invalid order data.");
                ViewBag.Products = await _context.Products.ToListAsync();
                return View(order);
            }

            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToListAsync();

            for (int i = 0; i < productIds.Length; i++)
            {
                var product = products.FirstOrDefault(p => p.ProductId == productIds[i]);
                if (product == null || quantities[i] > product.Quantity)
                {
                    ModelState.AddModelError("", $"Not enough stock for {product?.Name ?? "Unknown"}.");
                    ViewBag.Products = await _context.Products.ToListAsync();
                    return View(order);
                }
            }

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
                return RedirectToAction("Summary", new { id = existingOrder.OrderId });
            }
            else
            {
                order.OrderId = await _context.Orders.AnyAsync() ? await _context.Orders.MaxAsync(o => o.OrderId) + 1 : 1;
                order.ProductIds = productIds;
                order.Quantities = quantities;

                foreach (var item in productIds.Select((pid, i) => new { pid, qty = quantities[i] }))
                {
                    var product = products.First(p => p.ProductId == item.pid);
                    product.Quantity -= item.qty;
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction("Summary", new { id = order.OrderId });
            }
        }

        [HttpGet("Summary")]
        public async Task<IActionResult> Summary(int id)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null) return NotFound();

            ViewBag.Products = await _context.Products.ToListAsync();
            return View(order);
        }

        [HttpGet("Track")]
        public async Task<IActionResult> Track()
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
                        return View("Track");

                    ViewBag.Products = await _context.Products.ToListAsync();
                    return View("Summary", orders.First());
                }
            }

            return View(); // for Admin: allow manual entry
        }

        [HttpPost("Track")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Track(string customerName, string customerEmail)
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
    }
}