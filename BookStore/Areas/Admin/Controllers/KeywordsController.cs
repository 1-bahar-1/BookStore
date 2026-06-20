using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class KeywordsController : Controller
{
    private readonly ApplicationDbContext _context;

    public KeywordsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var keywords = await _context.Keywords
            .Include(k => k.BookKeywords)
            .ThenInclude(bk => bk.Book)
            .ToListAsync();

        return View(keywords);
    }

    public IActionResult Create() => View(new Keyword());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Keyword keyword)
    {
        if (ModelState.IsValid)
        {
            _context.Keywords.Add(keyword);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(keyword);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var keyword = await _context.Keywords.FindAsync(id);
        if (keyword == null)
            return NotFound();

        return View(keyword);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Keyword keyword)
    {
        if (id != keyword.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(keyword);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(keyword);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var keyword = await _context.Keywords
            .Include(k => k.BookKeywords)
            .FirstOrDefaultAsync(k => k.Id == id);

        if (keyword == null)
            return NotFound();

        return View(keyword);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var keyword = await _context.Keywords
            .Include(k => k.BookKeywords)
            .FirstOrDefaultAsync(k => k.Id == id);

        if (keyword != null)
        {
            _context.BookKeywords.RemoveRange(keyword.BookKeywords);
            _context.Keywords.Remove(keyword);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
