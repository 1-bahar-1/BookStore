using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers;

public class AuthorController : Controller
{
    private readonly ApplicationDbContext _context;

    public AuthorController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("authors/{slug}")]
    [Authorize]
    public async Task<IActionResult> Details(string slug)
    {
        var author = await _context.Authors
            .FirstOrDefaultAsync(a => a.Slug == slug);

        if (author == null)
            return NotFound();

        var books = await _context.Books
            .Include(b => b.Category)
            .Include(b => b.BookAuthors)
            .ThenInclude(ba => ba.Author)
            .Where(b => b.BookAuthors.Any(ba => ba.AuthorId == author.Id))
            .OrderByDescending(b => b.Id)
            .ToListAsync();

        ViewBag.AuthorName = author.FullName;
        return View(books);
    }
}
