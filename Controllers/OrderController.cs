using COMP2139_Assignment1.Views.Order;

namespace COMP2139_Assignment1.Controllers;
using System.Linq;
using Data;
using Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[Route("[controller]/[action]")]

public class OrderController : Controller
{
    private readonly ApplicationDbContext _context;

    public OrderController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [HttpGet("")]
    [Route("Order")]
    [Authorize(Roles = "Admin")]
    public IActionResult Index()
    {
        var orders = _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ToList();

        foreach (var order in orders)
        {
            order.TotalPrice = order.OrderItems.Sum(oi => oi.Product!.Price * oi.Quantity);
        }

        return View(orders.ToList());
    }
    
    [HttpGet("Create")]
    [Authorize(Roles = "Admin,User")]
    public IActionResult Create()
    {
        var viewModel = new OrderCreateViewModel
        {
            Order = new Order(),
            Products = _context.Products.ToList()
        };

        return View(viewModel);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> Create(OrderCreateViewModel model)
    {
        // Print out what was posted — helps us debug
        Console.WriteLine("SelectedProducts: " + string.Join(", ", model.SelectedProducts));
        Console.WriteLine("ProductQuantities: " + string.Join(", ", model.ProductQuantities));

        // Align selected products with their quantities
        var selectedQuantities = model.SelectedProducts
            .Select((id, index) => index < model.ProductQuantities.Count ? model.ProductQuantities[index] : 0)
            .ToList();

        // Validate
        if (!ModelState.IsValid || !model.SelectedProducts.Any() || selectedQuantities.All(q => q <= 0))
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            Console.WriteLine("Validation errors: " + string.Join("; ", errors));
            return Json(new { success = false, message = "Invalid form data." });
        }

        var order = new Order
        {
            GuestName = model.GuestName,
            GuestEmail = model.GuestEmail,
            OrderDate = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(); // Save to get Order ID

        for (int i = 0; i < model.SelectedProducts.Count; i++)
        {
            int productId = model.SelectedProducts[i];
            int quantity = selectedQuantities[i];

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

        await _context.SaveChangesAsync();
        return Json(new { success = true, redirectUrl = Url.Action("Confirm", "Order", new { id = order.Id }) });

    }
    /*{
        if (ModelState.IsValid)
        {
            model.Products = _context.Products.ToList();
            return View(model);
        }

        var order = new Order
        {
            GuestName = model.GuestName,
            GuestEmail = model.GuestEmail,
            OrderDate = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        if (model.SelectedProducts.Any() && model.ProductQuantities.Any())
        {
            // block to catch mismatched input
            if (model.SelectedProducts.Count != model.ProductQuantities.Count)
            {
                ModelState.AddModelError("", "Mismatch in products and quantities.");
                model.Products = _context.Products.ToList();
                return View(model);
            }
            
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

        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }*/
    
    [HttpGet("Confirm/{id}")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> Confirm(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        return View(order);
    }

    [HttpGet("UpdateStatus/{orderId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int orderId)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
        {
            ModelState.AddModelError(string.Empty, "Order not found.");
            return RedirectToAction("Index");
        }

        var viewModel = new OrderCreateViewModel
        {
            Id = order.Id,
            OrderStatus = order.OrderStatus
        };

        return View(viewModel);
    }

    [HttpPost("UpdateStatus/{orderId},{orderStatus}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int orderId, string orderStatus)
    {
        var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order != null)
        {
            order.OrderStatus = orderStatus;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        return NotFound();
    }

    [HttpGet("Track")]
    [Authorize(Roles = "Admin,User")]
    public IActionResult Track()
    {
        return View();
    }

    [HttpPost("Track/{orderId}")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> Track(string orderId)
    {
        if (string.IsNullOrEmpty(orderId))
        {
            ViewData["Error"] = "Please enter a valid Order ID.";
            return View();
        }

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id.ToString() == orderId);

        if (order == null)
        {
            ViewData["Error"] = "Order not found.";
            return View();
        }

        order.TotalPrice = order.OrderItems.Sum(oi => oi.Product!.Price * oi.Quantity);
        return View(order);
    }
    [HttpGet]
    [Route("Order/Delete/{id}")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        _context.OrderItems.RemoveRange(order.OrderItems); // Remove order items first
        _context.Orders.Remove(order);                     // Then remove the order itself
        await _context.SaveChangesAsync();

        return RedirectToAction("Index"); // Or return JSON if you're doing AJAX
    }
    
}














