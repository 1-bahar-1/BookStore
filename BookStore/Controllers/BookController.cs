using BookStore.Data;
using BookStore.Models;
using BookStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookStore.Controllers;
[Authorize]
public class BookController : Controller
{
    private readonly IBookRepository _bookRepository;
    private readonly ApplicationDbContext _context;

    public BookController(IBookRepository bookRepository, ApplicationDbContext context)
    {
        _bookRepository = bookRepository;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var books = await _bookRepository.GetAllAsync();
        return View(books);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
        return View();
    }

    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Book book)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View(book);
        }

        await _bookRepository.AddAsync(book);
        return RedirectToAction("Index");
    }
    public async Task<IActionResult> Details(string slug)
    {
        var book = await _bookRepository.GetBySlugAsync(slug);
        if (book == null) return NotFound();

        return View(book);
    }

}