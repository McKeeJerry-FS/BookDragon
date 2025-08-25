using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookDragon.Data;
using BookDragon.Models;
using BookDragon.Services.Interfaces;

namespace BookDragon.Controllers
{
    [Authorize]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;

        public BooksController(ApplicationDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        // GET: Books
        public async Task<IActionResult> Index(string? search, int? categoryId, bool? haveRead, bool? wishlist)
        {
            var query = _context.Books
                .Include(b => b.Category)
                .Include(b => b.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(b => (b.Title != null && EF.Functions.ILike(b.Title, $"%{search}%"))
                                    || (b.Author != null && EF.Functions.ILike(b.Author, $"%{search}%"))
                                    || (b.Description != null && EF.Functions.ILike(b.Description, $"%{search}%")));
            }
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(b => b.CategoryId == categoryId.Value);
            }
            if (haveRead.HasValue)
            {
                query = query.Where(b => b.HaveRead == haveRead.Value);
            }
            if (wishlist.HasValue)
            {
                query = query.Where(b => b.IsWishlist == wishlist.Value);
            }

            ViewData["Categories"] = new SelectList(_context.Categories.OrderBy(c => c.Name).ToList(), "Id", "Name", categoryId);
            ViewData["Search"] = search;
            ViewData["FilterCategoryId"] = categoryId;
            ViewData["FilterHaveRead"] = haveRead;
            ViewData["FilterWishlist"] = wishlist;

            var results = await query.ToListAsync();
            return View(results);
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            // Provide a non-null model instance so the Create view's Model references (image preview, etc.) don't throw
            return View(new Book());
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Title,Author,Genre,PublishedDate,Description,PageCount,CategoryId,BookType,Rating,RatingReason,HaveRead,IsWishlist,ImageFile,ImageData,ImageType,UserId")] Book book)
        {
            // Normalize strings
            book.Title = book.Title?.Trim();
            book.Author = book.Author?.Trim();
            book.Description = book.Description?.Trim();
            book.RatingReason = book.RatingReason?.Trim();

            // Convert new upload regardless of model validity so preview persists
            if (book.ImageFile != null)
            {
                book.ImageData = await _imageService.ConvertFileToByteArrayAsynC(book.ImageFile);
                book.ImageType = book.ImageFile.ContentType;
            }

            // If no file but ImageData posted back (hidden field after prior failed validation) keep it
            // (Model binder already populated ImageData from base64 hidden field if present)

            if (ModelState.IsValid)
            {
                if (book.PublishedDate != default && book.PublishedDate.Kind == DateTimeKind.Unspecified)
                {
                    book.PublishedDate = DateTime.SpecifyKind(book.PublishedDate, DateTimeKind.Utc);
                }
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", book.UserId);
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", book.UserId);
            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,Genre,PublishedDate,Description,PageCount,CategoryId,BookType,Rating,RatingReason,HaveRead,IsWishlist,ImageFile,ImageData,ImageType")] Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            // Fetch existing entity to allow selective updates and persistence of unchanged values
            var existingBook = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (existingBook == null)
            {
                return NotFound();
            }

            // Preserve immutable fields (UserId) and existing image before validation
            book.UserId = existingBook.UserId; // keep original owner
            if (ModelState.ContainsKey("UserId")) ModelState.Remove("UserId");

            // Normalize input (trim strings)
            book.Title = book.Title?.Trim();
            book.Author = book.Author?.Trim();
            book.Description = book.Description?.Trim();
            book.RatingReason = book.RatingReason?.Trim();

            // If a new file uploaded convert immediately so preview persists even on validation failure
            if (book.ImageFile != null)
            {
                book.ImageData = await _imageService.ConvertFileToByteArrayAsynC(book.ImageFile);
                book.ImageType = book.ImageFile.ContentType;
            }
            else if (book.ImageData == null && existingBook.ImageData != null)
            {
                // Preserve existing image for redisplay
                book.ImageData = existingBook.ImageData;
                book.ImageType = existingBook.ImageType;
            }

            // Additional defensive validation (in case client-side disabled)
            if (string.IsNullOrWhiteSpace(book.Title))
            {
                ModelState.AddModelError("Title", "Title is required");
            }
            if (string.IsNullOrWhiteSpace(book.Author))
            {
                ModelState.AddModelError("Author", "Author is required");
            }
            if (string.IsNullOrWhiteSpace(book.Description))
            {
                ModelState.AddModelError("Description", "Description is required");
            }
            if (book.CategoryId <= 0)
            {
                ModelState.AddModelError("CategoryId", "Please select a category");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update only modifiable fields (prevent accidental overwrites)
                    existingBook.Title = book.Title;
                    existingBook.Author = book.Author;
                    existingBook.Genre = book.Genre;
                    existingBook.PublishedDate = book.PublishedDate; // if enabled later
                    existingBook.Description = book.Description;
                    existingBook.PageCount = book.PageCount;
                    existingBook.CategoryId = book.CategoryId;
                    existingBook.BookType = book.BookType;
                    existingBook.Rating = book.Rating;
                    existingBook.RatingReason = book.RatingReason;
                    existingBook.HaveRead = book.HaveRead;
                    existingBook.IsWishlist = book.IsWishlist;

                    // Image: replace only if new file provided
                    if (book.ImageFile != null)
                    {
                        existingBook.ImageData = await _imageService.ConvertFileToByteArrayAsynC(book.ImageFile);
                        existingBook.ImageType = book.ImageFile.ContentType;
                    }

                    // Do not alter UserId or ImageData/ImageType (unless new image)
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", book.UserId);
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}