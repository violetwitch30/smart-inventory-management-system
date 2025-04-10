using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryManagementSystem.Areas.ProductManagement.Models;
using SmartInventoryManagementSystem.Models;

namespace SmartInventoryManagementSystem.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("HomeController Index action called at {Time}", DateTime.Now);
        return View();
    }

    public IActionResult About()
    {
        _logger.LogInformation("HomeController About action called at {Time}", DateTime.Now);
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    
    [HttpGet]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    public IActionResult NotFound(int statusCode)
    {
        if (statusCode == 404)
        {
            return View("NotFound");
        }

        return View("Error");
    }
}