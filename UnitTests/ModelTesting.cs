using System.ComponentModel.DataAnnotations;
using SmartInventoryManagementSystem.Areas.ProductManagement.Models;

namespace SmartInventoryManagementSystem.Tests
{
    public class ProductModelTests
    {
        [Fact]
        public void ProductValidation_Fails_WhenNameIsEmpty()
        {
            var product = new Product 
            { 
                Name = "", 
                Price = 10, 
                Quantity = 5 
            };
            
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(product, null, null);
            var isValid = Validator.TryValidateObject(product, context, validationResults, true);
            
            Assert.False(isValid);
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Name"));
        }
    }
}