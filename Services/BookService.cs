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
    throw new NotImplementedException();
  }

  public Task DeleteBookAsync(int id)
  {
    throw new NotImplementedException();
  }

  public Task<IEnumerable<Book>> GetAllBooksAsync(string userId)
  {
    return Task.FromResult(_context.Books
      .Where(b => b.UserId == userId)
      .AsEnumerable());
  }

  public Task<Book> GetBookByIdAsync(int id)
  {
    throw new NotImplementedException();
  }

  public Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
  {
    throw new NotImplementedException();
  }

  public Task UpdateBookAsync(Book book)
  {
    throw new NotImplementedException();
  }
}
