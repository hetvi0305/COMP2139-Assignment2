using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COMP2139_Assignment1.Models;

public class OrderItem
{
    [Key]
    public int Id { get; set; }
        
    [ForeignKey("OrderId")]
    public int OrderId { get; set; }
    public Order? Order { get; set; }

    [ForeignKey("ProductId")]
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }
}