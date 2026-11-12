using Library_Web_App.Data;
using Library_Web_App.Models;
using Library_Web_App.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Library_Web_App.Controllers;

public class AccountsController : Controller
{
    private readonly LibraryDbContext _db;

    public AccountsController(LibraryDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login");

        var user = _db.Users.Find(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))!;
        return View(user);
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (await _db.Users.AnyAsync(u => u.Username == model.Username))
        {
            ModelState.AddModelError("Username", "Username is already taken.");
            return View(model);
        }

        if (await _db.Users.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError("Email", "Email is already in use.");
            return View(model);
        }

        var user = new AppUser
        {
            Username = model.Username,
            Email = model.Email,
            FullName = model.FullName,
            PasswordHash = PasswordHasher.Hash(model.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == model.Username);

        if (user == null || !PasswordHasher.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        if (user.IsBanned)
        {
            ModelState.AddModelError(string.Empty, "Your account has been suspended. Contact support.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.GivenName, user.FullName)
        };

        if (user.IsAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Edit()
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login");

        var user = _db.Users.Find(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))!;
        var model = new EditProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = _db.Users.Find(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))!;
        user.FullName = model.FullName;
        user.Email = model.Email;

        if (!string.IsNullOrEmpty(model.NewPassword))
        {
            user.PasswordHash = PasswordHasher.Hash(model.NewPassword);
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Delete()
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = _db.Users.Find(userId)!;
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}