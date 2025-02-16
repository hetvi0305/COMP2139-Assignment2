namespace COMP2139_Assignment1.Controllers;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COMP2139_Assignment1.Data;
using COMP2139_Assignment1.Models;


    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var categories = _context.Categories.ToList();
    
            // Ensure we pass a valid list to the view
            if (categories == null)
            {
                categories = new List<Category>();
            }

            return View(categories);
        }

        [HttpGet]
        public  IActionResult Details(int id)
        {
            var category =  _context.Categories.FirstOrDefault(m => m.Id == id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        [HttpGet]
        public IActionResult Edit(int id)
        {
           

            var category =  _context.Categories.Find(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // GET: Category/Delete/5
        [HttpGet]
        public IActionResult Delete(int id)
        {
            

            var category = _context.Categories.FirstOrDefault(m => m.Id == id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var category =  _context.Categories.Find(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                 _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
