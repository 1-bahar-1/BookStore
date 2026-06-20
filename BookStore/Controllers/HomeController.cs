using BookStore.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var latestBooks = await _context.Books
            .Include(b => b.Category)
            .OrderByDescending(b => b.Id)
            .Take(10)
            .ToListAsync();

        return View(latestBooks);
    }
}