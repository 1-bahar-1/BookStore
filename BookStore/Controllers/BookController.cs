using BookStore.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers;

public class BookController : Controller
{
    private readonly ApplicationDbContext _context;

    public BookController(ApplicationDbContext context)
    {
        _context = context;
    }

    
    [Authorize]
    public async Task<IActionResult> Index(string? q)
    {
        var query = _context.Books
            .Include(b => b.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(b => b.Title.Contains(q));
        }

        var books = await query
            .OrderByDescending(b => b.Id)
            .ToListAsync();

        return View(books);
    }

    
    [HttpGet("/books/{slug}")]
    [Authorize]
    public async Task<IActionResult> Details(string slug)
    {
        var book = await _context.Books
            .Include(b => b.Category)
            .Include(b => b.BookAuthors)
            .ThenInclude(ba => ba.Author)
            .Include(b => b.BookKeywords)
            .ThenInclude(bk => bk.Keyword)
            .Include(b => b.RelatedTo)
            .ThenInclude(rt => rt.RelatedBook)
            .FirstOrDefaultAsync(b => b.Slug == slug);

        if (book == null)
            return NotFound();

        return View(book);
    }

    
    [Authorize]
    public async Task<IActionResult> Download(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null || string.IsNullOrEmpty(book.FilePath))
            return NotFound();

        var filePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            book.FilePath.TrimStart('/')
        );

        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

        return File(fileBytes, "application/pdf", $"{book.Slug}.pdf");
    }
}