using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers;

public class KeywordController : Controller
{
    private readonly ApplicationDbContext _context;

    public KeywordController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("keywords/{word}")]
    [Authorize]
    public async Task<IActionResult> Details(string word)
    {
        var keyword = await _context.Keywords
            .FirstOrDefaultAsync(k => k.Word == word);

        if (keyword == null)
            return NotFound();

        var books = await _context.Books
            .Include(b => b.Category)
            .Include(b => b.BookKeywords)
            .ThenInclude(bk => bk.Keyword)
            .Where(b => b.BookKeywords.Any(bk => bk.KeywordId == keyword.Id))
            .OrderByDescending(b => b.Id)
            .ToListAsync();

        ViewBag.KeywordWord = keyword.Word;
        return View(books);
    }
}
