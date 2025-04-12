using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace COMP2139_Assignment1.Controllers;


using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data;
using Models;
[Route("Product")]
public class ProductController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Admin, User")]
    [HttpGet("")]
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
                products = products.OrderByDescending(p => p.Category!.Name);
                break;
            default:
                products = products.OrderBy(p => p.Category!.Name);
                break;
        }

        
        ViewBag.Categories = _context.Categories
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList();
        
        return View( products.ToList());
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("Create")]
    public IActionResult Create()
    {
        ViewBag.Categories = _context.Categories
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList();
        
        return View();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("Create/{product}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == product.CategoryId);

        if (category != null)
        {
            product.Category = category;
            await _context.SaveChangesAsync();
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
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        ViewBag.Categories = _context.Categories
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList();
        
        return View(product);
    }

    [Authorize(Roles = "Admin,User")]
    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        ViewBag.Categories = _context.Categories
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList();
        return View(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id, Name, CategoryId, Price, Quantity, LowStockThreshold")] Product product)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == product.CategoryId);

        if (category != null)
        {
            product.Category = category;
            await _context.SaveChangesAsync();
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
                await _context.SaveChangesAsync();

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
    
    [Authorize(Roles = "Admin,User")]
    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.Id == id);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("Delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.Include(p=>p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { id = product.Id });
        }

        return NotFound();
    }
    
    [Authorize(Roles = "Admin, User")]
    [HttpGet("Search")]
        public IActionResult Search(string search, int? categoryId, decimal? minPrice, decimal? maxPrice, string sortOrder, bool? lowStock)
        {
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            // Filtering
            if (!string.IsNullOrWhiteSpace(search))
            {
                string loweredSearch = search.ToLower();
                products = products.Where(p =>
                    p.Name.ToLower().Contains(loweredSearch) ||
                    p.Category.Name.ToLower().Contains(loweredSearch)
                );
            }
            
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

            // ViewBag.Categories is not needed for partial view unless it's reused in partial
            return PartialView("ProductListPartial", products.ToList());
        }

}













