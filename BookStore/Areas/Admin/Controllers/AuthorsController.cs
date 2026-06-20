using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AuthorsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AuthorsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var authors = await _context.Authors
            .Include(a => a.BookAuthors)
            .ThenInclude(ba => ba.Book)
            .ToListAsync();

        return View(authors);
    }

    public IActionResult Create() => View(new Author());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Author author)
    {
        if (ModelState.IsValid)
        {
            _context.Authors.Add(author);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(author);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var author = await _context.Authors.FindAsync(id);
        if (author == null)
            return NotFound();

        return View(author);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Author author)
    {
        if (id != author.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(author);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(author);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var author = await _context.Authors
            .Include(a => a.BookAuthors)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (author == null)
            return NotFound();

        return View(author);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var author = await _context.Authors
            .Include(a => a.BookAuthors)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (author != null)
        {
            _context.BookAuthors.RemoveRange(author.BookAuthors);
            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
