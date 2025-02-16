using COMP2139_Assignment1.Models;

namespace COMP2139_Assignment1.Controllers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


public class OrderItem
{
    [Key]
    public int Id { get; set; }
        
    [ForeignKey("OrderId")]
    public int OrderId { get; set; }
    public Order Order { get; set; }

    [ForeignKey("ProductId")]
    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int Quantity { get; set; }
}