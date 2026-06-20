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

    // GET: Admin/Books
    public async Task<IActionResult> Index()
    {
        var books = await _context.Books
            .Include(b => b.Category)
            .ToListAsync();

        return View(books);
    }

    // GET: Admin/Books/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View();
    }

    // POST: Admin/Books/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Book book, IFormFile? pdfFile)
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
                    return View(book);
                }

                book.FilePath = uploadResult.FilePath;
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View(book);
    }

    // GET: Admin/Books/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            return NotFound();
        }

        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View(book);
    }

    // POST: Admin/Books/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Book book, IFormFile? pdfFile)
    {
        if (id != book.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var existingBook = await _context.Books.FindAsync(id);

            if (existingBook == null)
            {
                return NotFound();
            }

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
                    return View(book);
                }

                // حذف فایل قبلی در صورت وجود
                DeletePhysicalFile(existingBook.FilePath);

                existingBook.FilePath = uploadResult.FilePath;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View(book);
    }

    // GET: Admin/Books/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var book = await _context.Books
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
        {
            return NotFound();
        }

        return View(book);
    }

    // POST: Admin/Books/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            return NotFound();
        }

        DeletePhysicalFile(book.FilePath);

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task<(bool Success, string? FilePath, string? ErrorMessage)> UploadPdfFile(IFormFile pdfFile)
    {
        var extension = Path.GetExtension(pdfFile.FileName).ToLower();

        if (extension != ".pdf")
        {
            return (false, null, "فقط فایل PDF مجاز است.");
        }

        if (pdfFile.ContentType != "application/pdf")
        {
            return (false, null, "نوع فایل معتبر نیست. فقط PDF مجاز است.");
        }

        var filesFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "files"
        );

        if (!Directory.Exists(filesFolder))
        {
            Directory.CreateDirectory(filesFolder);
        }

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
        {
            return;
        }

        var relativePath = filePath.TrimStart('/');

        var fullPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            relativePath
        );

        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }
}
