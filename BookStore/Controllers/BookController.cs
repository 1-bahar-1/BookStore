using BookStore.Data;
using BookStore.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookStore.Controllers;

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
    // GET: Book/Create
    public IActionResult Create()
    {
        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
        return View();
    }

    // POST: Book/Create
    [HttpPost]
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

}