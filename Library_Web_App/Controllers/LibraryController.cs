using Library_Web_App.Data;
using Library_Web_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Library_Web_App.Controllers;

public class LibraryController : Controller
{
    private readonly LibraryDbContext _db;
    private const int PageSize = 10;

    public LibraryController(LibraryDbContext db)
    {
        _db = db;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Index(string? search, int pg = 1)
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Accounts");

        var userId = GetUserId();
        var cartBookIds = await _db.CartItems
            .Where(c => c.UserId == userId)
            .Select(c => c.BookId)
            .ToListAsync();

        var activeRentalBookIds = await _db.Rentals
            .Where(r => r.ReturnDate == null)
            .Select(r => r.BookId)
            .ToListAsync();

        var query = _db.Books.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(b =>
                EF.Functions.ILike(b.Title, $"%{term}%") ||
                EF.Functions.ILike(b.Author, $"%{term}%"));
        }

        var totalBooks = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalBooks / (double)PageSize);
        if (pg < 1) pg = 1;
        if (pg > totalPages) pg = totalPages;

        var books = await query
            .OrderBy(b => b.Title)
            .Skip((pg - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var viewBooks = books.Select(b => new BookDisplayModel
        {
            Id = b.Id,
            Title = b.Title,
            Author = b.Author,
            Description = b.Description,
            InCart = cartBookIds.Contains(b.Id),
            OnLoan = activeRentalBookIds.Contains(b.Id)
        }).ToList();

        ViewBag.Search = search;
        ViewBag.Page = pg;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalBooks = totalBooks;

        return View(viewBooks);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int bookId)
    {
        var userId = GetUserId();

        var activeRental = await _db.Rentals
            .AnyAsync(r => r.BookId == bookId && r.ReturnDate == null);

        if (activeRental)
        {
            TempData["Error"] = "That book is currently on loan.";
            return RedirectToAction(nameof(Index));
        }

        var alreadyInCart = await _db.CartItems
            .AnyAsync(c => c.UserId == userId && c.BookId == bookId);

        if (!alreadyInCart)
        {
            _db.CartItems.Add(new CartItem { UserId = userId, BookId = bookId });
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromCart(int bookId)
    {
        var userId = GetUserId();

        var item = await _db.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.BookId == bookId);

        if (item != null)
        {
            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Cart()
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Accounts");

        var userId = GetUserId();
        var items = await _db.CartItems
            .Include(c => c.Book)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        return View(new CartViewModel { Items = items });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rent()
    {
        var userId = GetUserId();
        var cartItems = await _db.CartItems
            .Include(c => c.Book)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (cartItems.Count == 0)
        {
            TempData["Error"] = "Your cart is empty.";
            return RedirectToAction(nameof(Cart));
        }

        var now = DateTime.UtcNow;

        foreach (var item in cartItems)
        {
            var stillAvailable = await _db.Rentals
                .Where(r => r.BookId == item.BookId && r.ReturnDate == null)
                .CountAsync();

            if (stillAvailable > 0)
            {
                _db.CartItems.Remove(item);
                await _db.SaveChangesAsync();
                TempData["Error"] = $"\"{item.Book.Title}\" was just rented by someone else. It was removed from your cart.";
                return RedirectToAction(nameof(Cart));
            }

            _db.Rentals.Add(new Rental
            {
                UserId = userId,
                BookId = item.BookId,
                RentalDate = now,
                DueDate = now.AddDays(5)
            });
            _db.CartItems.Remove(item);
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = $"You have rented {cartItems.Count} book(s). Due in 5 days.";
        return RedirectToAction(nameof(MyRentals));
    }

    [HttpGet]
    public async Task<IActionResult> MyRentals()
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Accounts");

        var userId = GetUserId();
        var rentals = await _db.Rentals
            .Include(r => r.Book)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RentalDate)
            .ToListAsync();

        return View(new RentalViewModel { Rentals = rentals });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnBook(int rentalId)
    {
        var userId = GetUserId();

        var rental = await _db.Rentals
            .FirstOrDefaultAsync(r => r.Id == rentalId && r.UserId == userId);

        if (rental == null)
            return NotFound();

        if (rental.ReturnDate != null)
        {
            TempData["Error"] = "This book has already been returned.";
            return RedirectToAction(nameof(MyRentals));
        }

        rental.ReturnDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Book returned successfully.";
        return RedirectToAction(nameof(MyRentals));
    }
}