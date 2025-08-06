using System;
using BookDragon.Models;

namespace BookDragon.Services.Interfaces;

public interface IBookService
{
  Task<Book> GetBookByIdAsync(int id);
  Task<IEnumerable<Book>> GetAllBooksAsync();
  Task AddBookAsync(Book book);
  Task UpdateBookAsync(Book book);
  Task DeleteBookAsync(int id);
  Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm);
}
