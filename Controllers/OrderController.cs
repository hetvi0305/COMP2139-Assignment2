
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COMP2139_Assignment1.Data;
using COMP2139_Assignment1.Models;
using System.Linq;
using COMP2139_Assignment1.Controllers;

public class OrderController : Controller
{
    private readonly ApplicationDbContext _context;

    public OrderController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var orders = _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ToList();
        foreach (var order in orders)
        {
            order.TotalPrice = order.OrderItems.Sum(oi => oi.Product.Price * oi.Quantity);
        }

        return View(orders);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var viewModel = new OrderCreateViewModel
        {
            Order = new Order(), // Initialize Order
            Products = _context.Products.ToList() // Fetch available products
        };

        return View(viewModel);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
   
    public IActionResult Create(OrderCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            // If validation fails, reload products and return view
            model.Products = _context.Products.ToList();
            return View(model);
        }

        var order = new Order
        {
            GuestName = model.GuestName,
            GuestEmail = model.GuestEmail,
            OrderDate = DateTime.UtcNow // Use UTC to avoid PostgreSQL error
        };

        _context.Orders.Add(order);
        _context.SaveChanges(); // Save first to get OrderId

        // Ensure SelectedProducts and ProductQuantities have matching indexes
        if (model.SelectedProducts != null && model.ProductQuantities != null)
        {
            for (int i = 0; i < model.SelectedProducts.Count; i++)
            {
                var productId = model.SelectedProducts[i];
                var quantity = model.ProductQuantities[i];

                if (quantity > 0)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = productId,
                        Quantity = quantity
                    };

                    _context.OrderItems.Add(orderItem);
                }
            }
        }

        _context.SaveChanges(); // Save order items

        return RedirectToAction("Index"); // Redirect to order list
    }






    [HttpGet]
    public IActionResult Confirm(int id)
    {
        var order = _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefault(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }
    
    // This action renders the form for updating the order status.
    public IActionResult UpdateStatus(int orderId)
    {
        var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);

        if (order == null)
        {
            ModelState.AddModelError(string.Empty, "Order not found.");
            return RedirectToAction("Index");
        }

        // Create a ViewModel to pass to the view
        var viewModel = new OrderCreateViewModel
        {
            Id = order.Id,
            OrderStatus = order.OrderStatus // Make sure the current status is passed
        };

        return View(viewModel);
    }

[HttpPost]
[ValidateAntiForgeryToken]

public IActionResult UpdateStatus(int orderId, string orderStatus)
{
    // Find the order by its ID (you can use your DbContext to fetch it)
    var order = _context.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.Id == orderId);
    
    if (order != null)
    {
        // Update the status
        order.OrderStatus = orderStatus;
        
        // Save changes to the database
        _context.SaveChanges();
        
        // Redirect to the order index page after updating
        return RedirectToAction("Index");
    }

    // Handle the case where the order is not found (optional)
    return NotFound();
}


public IActionResult Track()
{
    return View();
}

[HttpPost]
public IActionResult Track(string orderId)
{
    if (string.IsNullOrEmpty(orderId))
    {
        ViewData["Error"] = "Please enter a valid Order ID.";
        return View();
    }

    var order = _context.Orders
        .Include(o => o.OrderItems)
        .ThenInclude(oi => oi.Product)
        .FirstOrDefault(o => o.Id.ToString() == orderId);

    if (order == null)
    {
        ViewData["Error"] = "Order not found.";
        return View();
    }

    // Calculate total price (if not already done in the model)
    order.TotalPrice = order.OrderItems.Sum(oi => oi.Product.Price * oi.Quantity);

    return View(order);  // Returning the order object with total price to Track view
}


}

