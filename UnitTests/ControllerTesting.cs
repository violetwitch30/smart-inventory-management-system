using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SmartInventoryManagementSystem.Areas.ProductManagement.Controllers;
using SmartInventoryManagementSystem.Areas.ProductManagement.Models;
using SmartInventoryManagementSystem.Data;

public class ProductControllerTests
{
    private ApplicationDbContext CreateContext(string name) =>
        new (new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(name).Options);

    [Fact]
    public async Task Add_Post_InvalidModel_ReturnsViewWithModel()
    {
        var ctx = CreateContext("AddInvalidModel");
        var logger = new Mock<ILogger<ProductController>>();
        var controller = new ProductController(ctx, logger.Object);
        controller.ModelState.AddModelError("Name", "Required");

        var product = new Product { Price = 10, Quantity = 1 };
        
        var result = await controller.Add(product);
        
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(product, viewResult.Model);
    }
}