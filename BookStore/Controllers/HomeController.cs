using BookStore.Data;
using Microsoft.AspNetCore.Authorization;
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

    // صفحه اصلی: لیست کتاب‌ها + جستجو
    public async Task<IActionResult> Index(string? q)
    {
        var query = _context.Books.Include(b => b.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(b => b.Title.Contains(q) );
        }

        var books = await query.OrderByDescending(b => b.Id).ToListAsync();
        return View(books);
    }

    // مشاهده جزئیات کتاب بر اساس Slug
    [Route("book/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var book = await _context.Books
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Slug == slug);

        if (book == null) return NotFound();

        return View(book);
    }

    // دانلود امن: فقط برای کاربران وارد شده
    [Authorize]
    public async Task<IActionResult> Download(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null || string.IsNullOrEmpty(book.FilePath))
            return NotFound("فایل یافت نشد.");

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", book.FilePath.TrimStart('/'));

        if (!System.IO.File.Exists(filePath))
            return NotFound("فایل فیزیکی روی سرور یافت نشد.");

        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(fileBytes, "application/pdf", $"{book.Slug}.pdf");
    }
}
