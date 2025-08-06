using System.Diagnostics;
using System.Security.Claims;
using BookDragon.Data;
using BookDragon.Models;
using BookDragon.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookDragon.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBookService _bookService;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, IBookService bookService, ApplicationDbContext context)
        {
            _logger = logger;
            _bookService = bookService;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult BookList()
        {
            return View();
        }

        [Authorize]
        public IActionResult AddBook()
        {
            return View();
        }

        [Authorize]
        public IActionResult AddCategory()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            if (ModelState.IsValid)
            {
                // Get the current user's ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Set the UserId for the book
                book.UserId = userId;
                
                // Add the book using the service
                await _bookService.AddBookAsync(book);
                
                // Redirect to BookList after successful creation
                return RedirectToAction(nameof(BookList));
            }
            ViewBag.Categories = _context.Set<Category>()
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList();
            // If model is not valid, return to the form with validation errors
            return View("AddBook", book);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
