using System;
using BookDragon.Data;
using BookDragon.Models;
using BookDragon.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookDragon.Services;

public class BookService : IBookService
{
  private readonly ApplicationDbContext _context;

  public BookService(ApplicationDbContext context)
  {
    _context = context;
  }
  public Task AddBookAsync(Book book)
  {
    _context.Books.Add(book);
    return _context.SaveChangesAsync();
  }

  public Task DeleteBookAsync(int id)
  {
    var book = _context.Books.Find(id);
    if (book != null)
    {
      _context.Books.Remove(book);
      return _context.SaveChangesAsync();
    }
    return Task.CompletedTask;
  }

  public Task<IEnumerable<Book>> GetAllBooksAsync(string userId)
  {
    return Task.FromResult(_context.Books
      .Where(b => b.UserId == userId)
      .AsEnumerable());
  }

  public async Task<Book> GetBookByIdAsync(string userId, int id)
  {
    var book = await _context.Books
      .Where(b => b.UserId == userId)
      .Include(b => b.User)
      .FirstOrDefaultAsync(b => b.Id == id);

    if (book == null)
      throw new InvalidOperationException("Book not found.");

    return book;
  }

  public Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
  {
    return Task.FromResult(_context.Books
      .Where(b => (b.Title != null && b.Title.Contains(searchTerm)) || (b.Author != null && b.Author.Contains(searchTerm)))
      .AsEnumerable());
  }

  public Task UpdateBookAsync(Book book)
  {
    _context.Books.Update(book);
    return _context.SaveChangesAsync();
  }
}
