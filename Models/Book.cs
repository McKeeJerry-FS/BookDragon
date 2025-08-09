using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookDragon.Models;

public class Book
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Title")]
    public string? Title { get; set; }

    [Display(Name = "Author")]
    public string? Author { get; set; }

    [Display(Name = "Genre")]
    public string? Genre { get; set; }

    [Display(Name = "Published Date")]
    public DateTime PublishedDate { get; set; }

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Page Count")]
    public int PageCount { get; set; }

    [Display(Name = "Cover Image URL")]
    public string? CoverImageUrl { get; set; }
    
    [Required]
    [Display(Name = "Category")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
    public int CategoryId { get; set; } // Foreign Key

    // Image Properties
    [NotMapped]
    public IFormFile? ImageFile { get; set; }
    public byte[]? ImageData { get; set; }
    public string? ImageType { get; set; }

    // Foreign key for the user who owns this book
    [Required]
    public string? UserId { get; set; }

    // Navigation property
    [ForeignKey("UserId")]
    public virtual AppUser? User { get; set; }
    public virtual Category? Category { get; set; }

}
