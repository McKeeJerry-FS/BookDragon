using Microsoft.AspNetCore.Mvc;
using BookDragon.Services.Interfaces;
using BookDragon.Models;
using System.Security.Claims;

namespace BookDragon.Controllers
{
    [Route("Book")]
    public class BookController : Controller
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        // GET: BookController
        public ActionResult Index()
        {
            return View();
        }

        // GET: BookController/GetAllBooks
        [HttpGet("GetAllBooks")]
        public async Task<ActionResult<IEnumerable<Book>>> GetAllBooks()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User ID not found.");
            }
            var books = await _bookService.GetAllBooksAsync(userId);
            return Ok(books);
        }

    }
}
