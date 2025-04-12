namespace COMP2139_Assignment1.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class Product
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string? Name { get; set; }

    [ForeignKey("Category")]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Required]
    public int Quantity { get; set; }

    public int LowStockThreshold { get; set; }  
}
