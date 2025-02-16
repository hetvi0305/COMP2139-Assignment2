using COMP2139_Assignment1.Controllers;

namespace COMP2139_Assignment1.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Order
{
    [Key]
    public int Id { get; set; }
    
    public string GuestName { get; set; }
    
    public string GuestEmail { get; set; }

    private DateTime _orderDate;
    public DateTime OrderDate
    {
        get => _orderDate;
        set => _orderDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }


    public decimal TotalPrice { get; set; }
    
    public string OrderStatus { get; set; } = "Pending";
    
    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    
}
