using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers;

public class CategoryController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("categories/{slug}")]
    [Authorize]
    public async Task<IActionResult> Details(string slug)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Slug == slug);

        if (category == null)
            return NotFound();

        var books = await _context.Books
            .Include(b => b.Category)
            .Where(b => b.CategoryId == category.Id)
            .OrderByDescending(b => b.Id)
            .ToListAsync();

        ViewBag.CategoryName = category.Name;
        return View(books);
    }
}
