namespace COMP2139_Assignment1.Controllers;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Data;
using Models;

[Route("Category")]
public class CategoryController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Allow all logged-in users to view categories
    [HttpGet("")]
    [Authorize(Roles = "Admin,User")]
    public IActionResult Index()
    {
        var categories = _context.Categories.ToList();
        return View(categories);
    }

    [HttpGet("Details/{id}")]
    [Authorize(Roles = "Admin,User")]
    public IActionResult Details(int id)
    {
        var category = _context.Categories
            .Include(c => c.Products)
            .FirstOrDefault(m => m.Id == id);
        if (category == null)
            return NotFound();

        return View(category);
    }

    // Create, Edit, Delete - restricted to Admins
    [HttpGet("Create")]
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost("Create/{category}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public IActionResult Create([Bind("Id,Name,Description")] Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        return View(category);
    }

    [HttpGet("Edit/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Edit(int id)
    {
        var category = _context.Categories.Find(id);
        if (category == null)
            return NotFound();

        return View(category);
    }

    [HttpPost("Edit/{id},{category}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public IActionResult Edit(int id, [Bind("Id,Name,Description")] Category category)
    {
        if (id != category.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(category);
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Categories.Any(e => e.Id == category.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction("Index");
        }

        return View(category);
    }

    [HttpGet("Delete/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int id)
    {
        var category = _context.Categories.FirstOrDefault(m => m.Id == id);
        if (category == null)
            return NotFound();

        return View(category);
    }

    [HttpPost, ActionName("DeleteConfirmed")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteConfirmed(int id)
    {
        var category = _context.Categories.Find(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }
}
