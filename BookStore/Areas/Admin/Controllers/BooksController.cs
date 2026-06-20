using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class BooksController : Controller
{
    private readonly ApplicationDbContext _context;

    public BooksController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var books = await _context.Books
            .Include(b => b.Category)
            .Include(b => b.BookAuthors)
            .ThenInclude(ba => ba.Author)
            .ToListAsync();

        return View(books);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _context.Categories.ToListAsync();
        ViewBag.Authors = await _context.Authors.ToListAsync();
        ViewBag.Keywords = await _context.Keywords.ToListAsync();
        ViewBag.AllBooks = await _context.Books.ToListAsync();
        return View(new Book());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Book book, int[] authorIds, int[] keywordIds, int[] relatedBookIds, IFormFile? pdfFile)
    {
        if (ModelState.IsValid)
        {
            if (pdfFile != null && pdfFile.Length > 0)
            {
                var uploadResult = await UploadPdfFile(pdfFile);
                if (!uploadResult.Success)
                {
                    ModelState.AddModelError("FilePath", uploadResult.ErrorMessage!);
                    ViewBag.Categories = await _context.Categories.ToListAsync();
                    ViewBag.Authors = await _context.Authors.ToListAsync();
                    ViewBag.Keywords = await _context.Keywords.ToListAsync();
                    ViewBag.AllBooks = await _context.Books.ToListAsync();
                    return View(book);
                }
                book.FilePath = uploadResult.FilePath;
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            if (authorIds != null && authorIds.Length > 0)
            {
                foreach (var authorId in authorIds)
                {
                    _context.BookAuthors.Add(new BookAuthor { BookId = book.Id, AuthorId = authorId });
                }
            }

            if (keywordIds != null && keywordIds.Length > 0)
            {
                foreach (var keywordId in keywordIds)
                {
                    _context.BookKeywords.Add(new BookKeyword { BookId = book.Id, KeywordId = keywordId });
                }
            }

            if (relatedBookIds != null && relatedBookIds.Length > 0)
            {
                foreach (var relatedBookId in relatedBookIds)
                {
                    _context.BookRelations.Add(new BookRelation { BookId = book.Id, RelatedBookId = relatedBookId });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Categories = await _context.Categories.ToListAsync();
        ViewBag.Authors = await _context.Authors.ToListAsync();
        ViewBag.Keywords = await _context.Keywords.ToListAsync();
        ViewBag.AllBooks = await _context.Books.ToListAsync();
        return View(book);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var book = await _context.Books
            .Include(b => b.BookAuthors)
            .Include(b => b.BookKeywords)
            .Include(b => b.RelatedTo)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
            return NotFound();

        ViewBag.Categories = await _context.Categories.ToListAsync();
        ViewBag.Authors = await _context.Authors.ToListAsync();
        ViewBag.Keywords = await _context.Keywords.ToListAsync();
        ViewBag.AllBooks = await _context.Books.Where(b => b.Id != id).ToListAsync();
        return View(book);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Book book, int[] authorIds, int[] keywordIds, int[] relatedBookIds, IFormFile? pdfFile)
    {
        if (id != book.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            var existingBook = await _context.Books
                .Include(b => b.BookAuthors)
                .Include(b => b.BookKeywords)
                .Include(b => b.RelatedTo)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (existingBook == null)
                return NotFound();

            existingBook.Title = book.Title;
            existingBook.Slug = book.Slug;
            existingBook.PageCount = book.PageCount;
            existingBook.CategoryId = book.CategoryId;

            if (pdfFile != null && pdfFile.Length > 0)
            {
                var uploadResult = await UploadPdfFile(pdfFile);
                if (!uploadResult.Success)
                {
                    ModelState.AddModelError("FilePath", uploadResult.ErrorMessage!);
                    ViewBag.Categories = await _context.Categories.ToListAsync();
                    ViewBag.Authors = await _context.Authors.ToListAsync();
                    ViewBag.Keywords = await _context.Keywords.ToListAsync();
                    ViewBag.AllBooks = await _context.Books.Where(b => b.Id != id).ToListAsync();
                    return View(book);
                }
                DeletePhysicalFile(existingBook.FilePath);
                existingBook.FilePath = uploadResult.FilePath;
            }

            _context.BookAuthors.RemoveRange(existingBook.BookAuthors);
            _context.BookKeywords.RemoveRange(existingBook.BookKeywords);
            _context.BookRelations.RemoveRange(existingBook.RelatedTo);

            if (authorIds != null && authorIds.Length > 0)
            {
                foreach (var authorId in authorIds)
                {
                    _context.BookAuthors.Add(new BookAuthor { BookId = existingBook.Id, AuthorId = authorId });
                }
            }

            if (keywordIds != null && keywordIds.Length > 0)
            {
                foreach (var keywordId in keywordIds)
                {
                    _context.BookKeywords.Add(new BookKeyword { BookId = existingBook.Id, KeywordId = keywordId });
                }
            }

            if (relatedBookIds != null && relatedBookIds.Length > 0)
            {
                foreach (var relatedBookId in relatedBookIds)
                {
                    _context.BookRelations.Add(new BookRelation { BookId = existingBook.Id, RelatedBookId = relatedBookId });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Categories = await _context.Categories.ToListAsync();
        ViewBag.Authors = await _context.Authors.ToListAsync();
        ViewBag.Keywords = await _context.Keywords.ToListAsync();
        ViewBag.AllBooks = await _context.Books.Where(b => b.Id != id).ToListAsync();
        return View(book);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var book = await _context.Books
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
            return NotFound();

        return View(book);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var book = await _context.Books
            .Include(b => b.BookAuthors)
            .Include(b => b.BookKeywords)
            .Include(b => b.RelatedTo)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book != null)
        {
            DeletePhysicalFile(book.FilePath);
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<(bool Success, string? FilePath, string? ErrorMessage)> UploadPdfFile(IFormFile pdfFile)
    {
        var extension = Path.GetExtension(pdfFile.FileName).ToLowerInvariant();

        if (extension != ".pdf")
            return (false, null, "فقط فایل PDF مجاز است.");

        if (pdfFile.ContentType != "application/pdf")
            return (false, null, "نوع فایل معتبر نیست. فقط PDF مجاز است.");

        var filesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");
        if (!Directory.Exists(filesFolder))
            Directory.CreateDirectory(filesFolder);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(filesFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await pdfFile.CopyToAsync(stream);
        }

        var relativePath = "/files/" + fileName;
        return (true, relativePath, null);
    }

    private void DeletePhysicalFile(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return;

        var relativePath = filePath.TrimStart('/');
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

        if (System.IO.File.Exists(fullPath))
            System.IO.File.Delete(fullPath);
    }
}
