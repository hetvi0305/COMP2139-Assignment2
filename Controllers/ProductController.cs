using Microsoft.AspNetCore.Mvc.Rendering;

namespace COMP2139_Assignment1.Controllers;


using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COMP2139_Assignment1.Data;
using COMP2139_Assignment1.Models;

public class ProductController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index(string sortOrder)
    {
        ViewBag.CategorySortParam = String.IsNullOrEmpty(sortOrder) ? "category_desc" : "";

        var products = _context.Products
            .Include(p => p.Category)
            .ToList()
            .AsQueryable();
        
        switch (sortOrder)
        {
            case "category_desc":
                products = products.OrderByDescending(p => p.Category.Name);
                break;
            default:
                products = products.OrderBy(p => p.Category.Name);
                break;
        }

        
        ViewBag.Categories = _context.Categories
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList();
        
        return View(products.ToList());
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Categories = _context.Categories
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList();
        
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Product product)
    {
        var category = _context.Categories.FirstOrDefault(c => c.Id == product.CategoryId);

        if (category != null)
        {
            product.Category = category;
            _context.SaveChanges();
        }
        
        ModelState.ClearValidationState("Category");
        TryValidateModel(product);

        if (product.LowStockThreshold < 10)
        {
            ModelState.AddModelError("LowStockThreshold", "Low Stock Threshold must be greater than 10.");
        }

        if (ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                Console.WriteLine(error.ErrorMessage);
            }

            _context.Products.Add(product);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        ViewBag.Categories = _context.Categories
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList();
        
        return View(product);
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var product = _context.Products
            .Include(p => p.Category)
            .FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }
    
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var product = _context.Products.Find(id);
        if (product == null)
        {
            return NotFound();
        }
        ViewBag.Categories = _context.Categories
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList();
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, [Bind("Id, Name, CategoryId, Price, Quantity, LowStockThreshold")] Product product)
    {
        var category = _context.Categories.FirstOrDefault(c => c.Id == product.CategoryId);

        if (category != null)
        {
            product.Category = category;
            _context.SaveChanges();
        }
        
        ModelState.ClearValidationState("Category");
        TryValidateModel(product);

        if (id != product.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                Console.WriteLine(error.ErrorMessage);
            }

            try
            {
                _context.Products.Update(product);
                _context.SaveChanges();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction("Index");
        }
        ViewBag.Categories = _context.Categories
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList();
        return View(product);
    }
    
    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.Id == id);
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var product = _context.Products.Include(p=>p.Category).FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var product = _context.Products.Find(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            _context.SaveChanges();
            return RedirectToAction("Index", new { Id = product.Id });
        }

        return NotFound();
    }
    
    [HttpGet]
    public IActionResult Search(string search, int? categoryId, decimal? minPrice, decimal? maxPrice, string sortOrder, bool? lowStock)
    {
        var products = _context.Products.Include(p => p.Category).AsQueryable();

        // Filtering
        if (!string.IsNullOrEmpty(search))
            products = products.Where(p => p.Name.Contains(search));

        if (categoryId.HasValue)
            products = products.Where(p => p.CategoryId == categoryId);

        if (minPrice.HasValue)
            products = products.Where(p => p.Price >= minPrice);

        if (maxPrice.HasValue)
            products = products.Where(p => p.Price <= maxPrice);

        if (lowStock.HasValue && lowStock.Value)
            products = products.Where(p => p.Quantity <= p.LowStockThreshold);

        // Sorting
        switch (sortOrder)
        {
            case "price_asc":
                products = products.OrderBy(p => p.Price);
                break;
            case "price_desc":
                products = products.OrderByDescending(p => p.Price);
                break;
            case "quantity_asc":
                products = products.OrderBy(p => p.Quantity);
                break;
            case "quantity_desc":
                products = products.OrderByDescending(p => p.Quantity);
                break;
            case "name":
                products = products.OrderBy(p => p.Name);
                break;
            default:
                products = products.OrderBy(p => p.Category.Name);
                break;
        }

        ViewBag.Categories = _context.Categories
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList();

        return View("Index", products.ToList()); // Reuse the same view for search results
    }

}
