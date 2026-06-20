using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private const int PageSize = 10;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? categoryId, int? authorId, int? keywordId, int page = 1)
    {
        var query = _context.Books
            .Include(b => b.Category)
            .Include(b => b.BookAuthors)
            .ThenInclude(ba => ba.Author)
            .Include(b => b.BookKeywords)
            .ThenInclude(bk => bk.Keyword)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(b => b.CategoryId == categoryId.Value);
        }

        if (authorId.HasValue)
        {
            query = query.Where(b => b.BookAuthors.Any(ba => ba.AuthorId == authorId.Value));
        }

        if (keywordId.HasValue)
        {
            query = query.Where(b => b.BookKeywords.Any(bk => bk.KeywordId == keywordId.Value));
        }

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

        var books = await query
            .OrderByDescending(b => b.Id)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        ViewBag.Authors = await _context.Authors.OrderBy(a => a.FullName).ToListAsync();
        ViewBag.Keywords = await _context.Keywords.OrderBy(k => k.Word).ToListAsync();
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;
        ViewBag.HasPreviousPage = page > 1;
        ViewBag.HasNextPage = page < totalPages;

        return View(books);
    }
}
