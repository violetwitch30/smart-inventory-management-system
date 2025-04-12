using System.ComponentModel.DataAnnotations;

namespace SmartInventoryManagementSystem.Areas.ProductManagement.Models;

public class Category
{
    [Key]
    public int CategoryId { get; set; }
    [Required]
    public string Name { get; set; }
    public List<Product> Products { get; set; }
}