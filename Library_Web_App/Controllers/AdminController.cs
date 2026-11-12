using Library_Web_App.Data;
using Library_Web_App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library_Web_App.Controllers;

[Authorize(Policy = "AdminOnly")]
public class AdminController : Controller
{
    private readonly LibraryDbContext _db;

    public AdminController(LibraryDbContext db)
    {
        _db = db;
    }

    // ─── Dashboard ───────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

        var rentals = await _db.Rentals
            .Include(r => r.Book)
            .Where(r => r.RentalDate >= sevenDaysAgo)
            .ToListAsync();

        var chartData = rentals
            .GroupBy(r => r.Book.Title)
            .Select(g => new RentalChartItem
            {
                BookTitle = g.Key,
                RentalCount = g.Count(),
                DaysRented = g.Sum(r =>
                {
                    var end = r.ReturnDate ?? DateTime.UtcNow;
                    return (int)Math.Ceiling((end - r.RentalDate).TotalDays);
                })
            })
            .OrderByDescending(c => c.RentalCount)
            .ToList();

        var model = new AdminDashboardViewModel
        {
            ChartData = chartData,
            TotalUsers = await _db.Users.CountAsync(),
            TotalBooks = await _db.Books.CountAsync(),
            ActiveRentals = await _db.Rentals.CountAsync(r => r.ReturnDate == null)
        };

        return View(model);
    }

    // ─── Books CRUD ──────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Books()
    {
        var books = await _db.Books.ToListAsync();
        return View(books);
    }

    [HttpGet]
    public IActionResult AddBook()
    {
        return View(new AdminBookViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddBook(AdminBookViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (await _db.Books.AnyAsync(b => b.Title == model.Title))
        {
            ModelState.AddModelError("Title", "A book with that title already exists.");
            return View(model);
        }

        _db.Books.Add(new Book
        {
            Title = model.Title,
            Author = model.Author,
            Description = model.Description
        });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Books));
    }

    [HttpGet]
    public async Task<IActionResult> EditBook(int id)
    {
        var book = await _db.Books.FindAsync(id);
        if (book == null) return NotFound();

        var model = new AdminBookViewModel
        {
            Title = book.Title,
            Author = book.Author,
            Description = book.Description
        };
        ViewBag.BookId = id;
        return View(model);
    }
    public async Task<IActionResult> EditBook(int id, AdminBookViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var book = await _db.Books.FindAsync(id);
        if (book == null) return NotFound();

        book.Title = model.Title;
        book.Author = model.Author;
        book.Description = model.Description;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Books));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var hasActiveRentals = await _db.Rentals.AnyAsync(r => r.BookId == id && r.ReturnDate == null);
        if (hasActiveRentals)
        {
            TempData["Error"] = "Cannot delete: this book has active rentals.";
            return RedirectToAction(nameof(Books));
        }

        var book = await _db.Books.FindAsync(id);
        if (book == null) return NotFound();

        _db.Books.Remove(book);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Books));
    }

    // ─── User Management ─────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Users()
    {
        var users = await _db.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
        return View(new AdminUserViewModel { Users = users });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BanUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsBanned = true;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsBanned = false;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Users));
    }
}