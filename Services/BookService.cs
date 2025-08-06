using System;
using BookDragon.Models;
using BookDragon.Services.Interfaces;

namespace BookDragon.Services;

public class BookService : IBookService
{
  public Task AddBookAsync(Book book)
  {
    throw new NotImplementedException();
  }

  public Task DeleteBookAsync(int id)
  {
    throw new NotImplementedException();
  }

  public Task<IEnumerable<Book>> GetAllBooksAsync()
  {
    throw new NotImplementedException();
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
