using System.ComponentModel.DataAnnotations;

namespace SmartInventoryManagementSystem.Areas.ProductManagement.Models;

public class Order
{
    [Key]
    public int OrderId { get; set; }
    [DataType(DataType.Date)]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    [Required]
    public string CustomerName { get; set; }

    [Required, EmailAddress]
    public string CustomerEmail { get; set; }

    // Store product IDs and corresponding quantities
    public int[] ProductIds { get; set; }
    public int[] Quantities { get; set; } 

    // Calculate total price based on products in order
    public float TotalPrice(List<Product> products)
    {
        float total = 0;
        for (int i = 0; i < ProductIds.Length; i++)
        {
            var product = products.FirstOrDefault(p => p.ProductId == ProductIds[i]);
            if (product != null)
            {
                total += product.Price * Quantities[i];
            }
        }
        return total;
    }
}