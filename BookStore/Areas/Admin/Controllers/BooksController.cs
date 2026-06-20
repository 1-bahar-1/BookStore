using BookStore.Areas.Admin.Models;
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
        var vm = new BookFormViewModel();
        await LoadFormDataAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookFormViewModel vm, IFormFile? pdfFile, int[]? AuthorIds, int[]? KeywordIds, int[]? RelatedBookIds)
    {
        if (!ModelState.IsValid)
        {
            await LoadFormDataAsync(vm);
            return View(vm);
        }

        var book = new Book
        {
            Title = vm.Title,
            Slug = vm.Slug,
            PageCount = vm.PageCount,
            CategoryId = vm.CategoryId
        };

        if (pdfFile != null && pdfFile.Length > 0)
        {
            var uploadResult = await UploadPdfFile(pdfFile);
            if (!uploadResult.Success)
            {
                ModelState.AddModelError("FilePath", uploadResult.ErrorMessage!);
                await LoadFormDataAsync(vm);
                return View(vm);
            }
            book.FilePath = uploadResult.FilePath;
        }

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        await SaveBookRelationsAsync(book.Id, vm);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
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

        var vm = new BookFormViewModel
        {
            Id = book.Id,
            Title = book.Title,
            Slug = book.Slug,
            PageCount = book.PageCount,
            CategoryId = book.CategoryId,
            FilePath = book.FilePath
        };

        await LoadFormDataAsync(vm);

        var selectedAuthorIds = new HashSet<int>((book.BookAuthors ?? new List<BookAuthor>()).Select(ba => ba.AuthorId));
        var selectedKeywordIds = new HashSet<int>((book.BookKeywords ?? new List<BookKeyword>()).Select(bk => bk.KeywordId));
        var selectedRelatedBookIds = new HashSet<int>((book.RelatedTo ?? new List<BookRelation>()).Select(rt => rt.RelatedBookId));

        PopulateSelections(vm, selectedAuthorIds, selectedKeywordIds, selectedRelatedBookIds);

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BookFormViewModel vm, IFormFile? pdfFile)
    {
        if (id != vm.Id)
            return NotFound();

        if (!ModelState.IsValid)
        {
            await LoadFormDataAsync(vm);
            return View(vm);
        }

        var existingBook = await _context.Books
            .Include(b => b.BookAuthors)
            .Include(b => b.BookKeywords)
            .Include(b => b.RelatedTo)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (existingBook == null)
            return NotFound();

        existingBook.Title = vm.Title;
        existingBook.Slug = vm.Slug;
        existingBook.PageCount = vm.PageCount;
        existingBook.CategoryId = vm.CategoryId;

        if (pdfFile != null && pdfFile.Length > 0)
        {
            var uploadResult = await UploadPdfFile(pdfFile);
            if (!uploadResult.Success)
            {
                ModelState.AddModelError("FilePath", uploadResult.ErrorMessage!);
                await LoadFormDataAsync(vm);
                return View(vm);
            }
            DeletePhysicalFile(existingBook.FilePath);
            existingBook.FilePath = uploadResult.FilePath;
        }

        _context.BookAuthors.RemoveRange(existingBook.BookAuthors);
        _context.BookKeywords.RemoveRange(existingBook.BookKeywords);
        _context.BookRelations.RemoveRange(existingBook.RelatedTo);

        await SaveBookRelationsAsync(id, vm);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
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

    private async Task LoadFormDataAsync(BookFormViewModel vm)
    {
        vm.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        vm.Authors = await _context.Authors.OrderBy(a => a.FullName).Select(a => new AuthorSelection { AuthorId = a.Id, FullName = a.FullName, IsSelected = false }).ToListAsync();
        vm.Keywords = await _context.Keywords.OrderBy(k => k.Word).Select(k => new KeywordSelection { KeywordId = k.Id, Word = k.Word, IsSelected = false }).ToListAsync();
        vm.RelatedBooks = await _context.Books
            .Where(b => b.Id != vm.Id)
            .OrderBy(b => b.Title)
            .Select(b => new RelatedBookSelection { BookId = b.Id, Title = b.Title ?? "", IsSelected = false })
            .ToListAsync();
    }

    private async Task SaveBookRelationsAsync(int bookId, BookFormViewModel vm)
    {
        var selectedAuthorIds = (vm.AuthorIds ?? Array.Empty<int>()).Distinct().ToList();
        var selectedKeywordIds = (vm.KeywordIds ?? Array.Empty<int>()).Distinct().ToList();
        var selectedRelatedBookIds = (vm.RelatedBookIds ?? Array.Empty<int>()).Distinct().ToList();

        foreach (var authorId in selectedAuthorIds)
        {
            _context.BookAuthors.Add(new BookAuthor { BookId = bookId, AuthorId = authorId });
        }

        foreach (var keywordId in selectedKeywordIds)
        {
            _context.BookKeywords.Add(new BookKeyword { BookId = bookId, KeywordId = keywordId });
        }

        foreach (var relatedBookId in selectedRelatedBookIds)
        {
            _context.BookRelations.Add(new BookRelation { BookId = bookId, RelatedBookId = relatedBookId });
        }

        await _context.SaveChangesAsync();
    }

    private BookFormViewModel MapToViewModel(Book book)
    {
        var selectedAuthorIds = (book.BookAuthors ?? new List<BookAuthor>()).Select(ba => ba.AuthorId).ToHashSet();
        var selectedKeywordIds = (book.BookKeywords ?? new List<BookKeyword>()).Select(bk => bk.KeywordId).ToHashSet();
        var selectedRelatedBookIds = (book.RelatedTo ?? new List<BookRelation>()).Select(rt => rt.RelatedBookId).ToHashSet();

        var vm = new BookFormViewModel
        {
            Id = book.Id,
            Title = book.Title,
            Slug = book.Slug,
            PageCount = book.PageCount,
            CategoryId = book.CategoryId,
            FilePath = book.FilePath
        };

        return vm;
    }

    private void PopulateSelections(BookFormViewModel vm, HashSet<int> selectedAuthorIds, HashSet<int> selectedKeywordIds, HashSet<int> selectedRelatedBookIds)
    {
        if (vm.Authors != null)
        {
            foreach (var author in vm.Authors)
            {
                author.IsSelected = selectedAuthorIds.Contains(author.AuthorId);
            }
        }

        if (vm.Keywords != null)
        {
            foreach (var keyword in vm.Keywords)
            {
                keyword.IsSelected = selectedKeywordIds.Contains(keyword.KeywordId);
            }
        }

        if (vm.RelatedBooks != null)
        {
            foreach (var relatedBook in vm.RelatedBooks)
            {
                relatedBook.IsSelected = selectedRelatedBookIds.Contains(relatedBook.BookId);
            }
        }
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
