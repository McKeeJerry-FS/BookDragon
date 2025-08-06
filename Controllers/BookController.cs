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

        // Other actions (Create, Edit, Delete) can be added here
        [HttpGet("Details/{id}")]
        public async Task<ActionResult<Book>> GetBookDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User ID not found.");
            }
            var book = await _bookService.GetBookByIdAsync(userId, id);
            if (book == null)
            {
                return NotFound();
            }
            return Ok(book);
        }

        [HttpPost("Create")]
        public async Task<ActionResult> CreateBook([FromBody] Book book)
        {
            if (book == null)
            {
                return BadRequest("Invalid book data.");
            }

            await _bookService.AddBookAsync(book);
            return CreatedAtAction(nameof(GetBookDetails), new { id = book.Id }, book);
        }

        [HttpPut("Edit/{id}")]
        public async Task<ActionResult> EditBook(int id, [FromBody] Book book)
        {
            if (book == null || book.Id != id)
            {
                return BadRequest("Invalid book data.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User ID not found.");
            }
            var existingBook = await _bookService.GetBookByIdAsync(userId, id);
            if (existingBook == null)
            {
                return NotFound();
            }

            await _bookService.UpdateBookAsync(book);
            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult> DeleteBook(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User ID not found.");
            }
            var existingBook = await _bookService.GetBookByIdAsync(userId, id);
            if (existingBook == null)
            {
                return NotFound();
            }

            await _bookService.DeleteBookAsync(id);
            return NoContent();
        }

    }
}
