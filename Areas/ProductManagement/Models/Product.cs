using System.ComponentModel.DataAnnotations;

namespace Assignment1.Areas.ProductManagement.Models;

public class Product
{
    [Key]
    public int ProductId { get; set; }
    [Required]
    public string Name { get; set; }
    public string? Description { get; set; }
    [Required]
    public float Price { get; set; }
    [Required]
    public int Quantity { get; set; }
    public int LowStockThreshold { get; set; } = 10; // default threshold

    [Required]
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    
    public bool IsLowStock => Quantity < LowStockThreshold;
}