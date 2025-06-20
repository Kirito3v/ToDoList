using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;
using ToDoList.Data;
using ToDoList.Models;

namespace ToDoList.Controllers
{
    public class AccountController : Controller
    {
        private readonly ToDoDataContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ToDoDataContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LogInModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var authUser = await _context.AuthUsers
                    .Include(au => au.UserData)
                    .FirstOrDefaultAsync(u => u.Username == model.Username);

                if (authUser == null || !VerifyPassword(model.Password, authUser.PasswordHash, authUser.Salt))
                {
                    ModelState.AddModelError(string.Empty, "Invalid username or password");
                    return View(model);
                }

                if (authUser.IsLocked)
                {
                    ModelState.AddModelError(string.Empty, "Account is locked. Please contact support.");
                    return View(model);
                }

                authUser.FailedLoginAttempts = 0;
                authUser.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("UserId", authUser.UserId.ToString());
                return RedirectToAction("Index", "Todo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                ModelState.AddModelError(string.Empty, "An error occurred during login");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (await _context.AuthUsers.AnyAsync(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Username already taken");
                    return View(model);
                }

                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already registered");
                    return View(model);
                }

                var salt = GenerateSalt();
                var passwordHash = HashPassword(model.Password, salt);

                var userData = new UserData
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email
                };

                var authUser = new UserModel
                {
                    Username = model.Username,
                    PasswordHash = passwordHash,
                    Salt = salt,
                    UserData = userData
                };

                _context.AuthUsers.Add(authUser);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("UserId", authUser.UserId.ToString());
                return RedirectToAction("Index", "Todo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                ModelState.AddModelError(string.Empty, "An error occurred during registration");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        #region Password Helpers
        private static string GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        private static string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = password + salt;
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private static bool VerifyPassword(string password, string storedHash, string salt)
        {
            var computedHash = HashPassword(password, salt);
            return computedHash == storedHash;
        }
        #endregion
    }
}
