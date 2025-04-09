using Microsoft.AspNetCore.Mvc;
using SmartInventoryManagementSystem.Data;
using SmartInventoryManagementSystem.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using SmartInventoryManagementSystem.Areas.ProductManagement.Models;

namespace SmartInventoryManagementSystem.Areas.ProductManagement.Controllers
{
    [Area("ProductManagement")]
    [Route("[area]/[controller]/[action]")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor
        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Order/Create (Display the order creation form)
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Products = _context.Products.ToList(); // load products
            return View();
        }

        // POST: Order/Create (Handle order creation)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Order order, int[] productIds, int[] quantities)
        {
            // Validate input arrays
            if (productIds == null || quantities == null || productIds.Length != quantities.Length)
            {
                ModelState.AddModelError("", "Invalid order items");
                ViewBag.Products = _context.Products.ToList();
                return View(order);
            }

            // Get products corresponding to the provided productIds
            var products = _context.Products.Where(p => productIds.Contains(p.ProductId)).ToList();

            // Validate product existence and stock availability
            for (int i = 0; i < productIds.Length; i++)
            {
                var product = products.FirstOrDefault(p => p.ProductId == productIds[i]);
                if (product == null)
                {
                    ModelState.AddModelError("", "Selected product does not exist.");
                    ViewBag.Products = _context.Products.ToList();
                    return View(order);
                }

                _context.Entry(product).Reload(); // update stock data

                if (quantities[i] > product.Quantity)
                {
                    ModelState.AddModelError("", $"Not enough stock for {product.Name}. Available: {product.Quantity}, Requested: {quantities[i]}.");
                    ViewBag.Products = _context.Products.ToList();
                    return View(order);
                }
            }

            // Check for an existing order by customer name and email
            var existingOrder = _context.Orders
                .FirstOrDefault(o => o.CustomerName == order.CustomerName && o.CustomerEmail == order.CustomerEmail);

            if (existingOrder != null)
            {
                // Convert existing arrays to lists for easier manipulation.
                var productIdsList = existingOrder.ProductIds?.ToList() ?? new List<int>();
                var quantitiesList = existingOrder.Quantities?.ToList() ?? new List<int>();

                for (int i = 0; i < productIds.Length; i++)
                {
                    var product = _context.Products.FirstOrDefault(p => p.ProductId == productIds[i]);
                    if (product != null)
                    {
                        int index = productIdsList.IndexOf(productIds[i]);
                        if (index >= 0)
                        {
                            // Check stock availability again if needed
                            if (quantities[i] > product.Quantity)
                            {
                                ModelState.AddModelError("", $"Not enough stock for {product.Name}.");
                                ViewBag.Products = _context.Products.ToList();
                                return View(order);
                            }
                            quantitiesList[index] += quantities[i];
                        }
                        else
                        {
                            productIdsList.Add(productIds[i]);
                            quantitiesList.Add(quantities[i]);
                        }

                        // Deduct the ordered quantity from the product's stock
                        product.Quantity -= quantities[i];
                    }
                }

                // Update the order arrays with the modified lists
                existingOrder.ProductIds = productIdsList.ToArray();
                existingOrder.Quantities = quantitiesList.ToArray();

                _context.Orders.Update(existingOrder);
            }
            else
            {
                // Create new order with a new OrderId
                order.OrderId = _context.Orders.Any() ? _context.Orders.Max(o => o.OrderId) + 1 : 1;
                order.ProductIds = productIds;
                order.Quantities = quantities;

                for (int i = 0; i < productIds.Length; i++)
                {
                    var product = _context.Products.FirstOrDefault(p => p.ProductId == productIds[i]);
                    if (product != null)
                    {
                        product.Quantity -= quantities[i];
                    }
                }

                _context.Orders.Add(order);
            }

            _context.SaveChanges();
            return RedirectToAction("Summary", new { id = existingOrder?.OrderId ?? order.OrderId });
        }

        // GET: Order/Summary (Display order summary)
        [HttpGet]
        public IActionResult Summary(int id)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            ViewBag.Products = _context.Products.ToList();
            return View(order);
        }

        // GET: Order/Track (Show the order tracking form)
        [HttpGet]
        public IActionResult Track()
        {
            return View();
        }

        // POST: Order/Track (Display order summary for the given customer)
        [HttpPost]
        public IActionResult Track(string customerName, string customerEmail)
        {
            if (string.IsNullOrEmpty(customerName) || string.IsNullOrEmpty(customerEmail))
            {
                ModelState.AddModelError("", "Name and email are required");
                return View();
            }

            var orders = _context.Orders
                .Where(o => o.CustomerName == customerName && o.CustomerEmail == customerEmail)
                .ToList();

            if (!orders.Any())
            {
                ModelState.AddModelError("", "No orders found for the given name and email");
                return View();
            }

            // Display summary for the latest order
            var order = orders.OrderByDescending(o => o.OrderDate).FirstOrDefault();
            ViewBag.Products = _context.Products.ToList();
            return View("Summary", order);
        }
    }
}