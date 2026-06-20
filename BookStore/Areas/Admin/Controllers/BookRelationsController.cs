using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class BookRelationsController : Controller
{
    private readonly ApplicationDbContext _context;

    public BookRelationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var relations = await _context.BookRelations
            .Include(br => br.Book)
            .Include(br => br.RelatedBook)
            .OrderByDescending(br => br.Id)
            .ToListAsync();

        return View(relations);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Books = await _context.Books.OrderBy(b => b.Title).ToListAsync();
        return View(new BookRelation());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookRelation relation)
    {
        if (relation.BookId == relation.RelatedBookId)
        {
            ModelState.AddModelError("", "کتاب و کتاب مرتبط نمی‌توانند یکی باشند.");
        }

        if (ModelState.IsValid)
        {
            _context.BookRelations.Add(relation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Books = await _context.Books.OrderBy(b => b.Title).ToListAsync();
        return View(relation);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var relation = await _context.BookRelations
            .Include(br => br.Book)
            .Include(br => br.RelatedBook)
            .FirstOrDefaultAsync(br => br.Id == id);

        if (relation == null)
            return NotFound();

        return View(relation);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var relation = await _context.BookRelations.FindAsync(id);
        if (relation != null)
        {
            _context.BookRelations.Remove(relation);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
