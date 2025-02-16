using COMP2139_Assignment1.Models;

public class OrderCreateViewModel
{
    public Order Order { get; set; } = new Order();
    public string GuestName { get; set; }
    public string GuestEmail { get; set; }
    public int Id { get; set; }
    public string OrderStatus { get; set; }
    public List<Product> Products { get; set; } = new List<Product>();

    public List<int> SelectedProducts { get; set; } = new List<int>(); // Stores product IDs
    public List<int> ProductQuantities { get; set; } = new List<int>(); // Stores quantities (must match SelectedProducts index)

}