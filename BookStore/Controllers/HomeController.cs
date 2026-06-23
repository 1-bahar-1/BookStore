using BookStore.Data;
using BookStore.Models;
using BookStore.ViewModels;
using Microsoft.AspNetCore.Authorization;
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

    [Authorize]
    public async Task<IActionResult> Index(
        int? categoryId,
        int? authorId,
        int? keywordId,
        string? q,
        int page = 1)
    {
        if (page < 1) page = 1;

        var baseQuery = _context.Books
            .AsNoTracking()
            .Include(b => b.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            baseQuery = baseQuery.Where(b =>
                b.Title.Contains(q) ||
                (b.Description != null && b.Description.Contains(q)));
        }

        if (categoryId.HasValue)
        {
            baseQuery = baseQuery.Where(b => b.CategoryId == categoryId.Value);
        }

        if (authorId.HasValue)
        {
            baseQuery = baseQuery.Where(b =>
                _context.BookAuthors.Any(ba => ba.BookId == b.Id && ba.AuthorId == authorId.Value));
        }

        if (keywordId.HasValue)
        {
            baseQuery = baseQuery.Where(b =>
                _context.BookKeywords.Any(bk => bk.BookId == b.Id && bk.KeywordId == keywordId.Value));
        }

        var totalItems = await baseQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

        if (totalPages == 0) totalPages = 1;
        if (page > totalPages) page = totalPages;

        var books = await baseQuery
            .OrderByDescending(b => b.Id)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(b => new BookCardDto
            {
                Id = b.Id,
                Title = b.Title,
                Slug = b.Slug,
                PageCount = b.PageCount,
                IsFree = b.IsFree,
                CoverImagePath = b.CoverImagePath,
                CategoryName = b.Category.Name
            })
            .ToListAsync();

        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        var authors = await _context.Authors
            .AsNoTracking()
            .OrderBy(a => a.FullName)
            .ToListAsync();

        var keywords = await _context.Keywords
            .AsNoTracking()
            .OrderBy(k => k.Word)
            .ToListAsync();

        var vm = new HomeIndexViewModel
        {
            Books = books,
            Categories = categories,
            Authors = authors,
            Keywords = keywords,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalItems = totalItems,
            HasPreviousPage = page > 1,
            HasNextPage = page < totalPages,
            SelectedCategoryId = categoryId,
            SelectedAuthorId = authorId,
            SelectedKeywordId = keywordId,
            SearchQuery = q
        };

        return View(vm);
    }
}
