using System.Collections.Generic;
using BookDragon.Models;

namespace BookDragon.ViewModels
{
    public class PagedBookListViewModel
    {
        public IEnumerable<Book> Books { get; set; } = new List<Book>();
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
    }
}
