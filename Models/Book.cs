using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookDragon.Enums;

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

    [Display(Name = "Book Type")]
    public BookType? BookType { get; set; }

    // Rating Properties
    [Display(Name = "Rating")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
    public int? Rating { get; set; }

    [Display(Name = "Rating Reason")]
    [StringLength(500, ErrorMessage = "Rating reason cannot exceed 500 characters")]
    public string? RatingReason { get; set; }

    // Image Properties
    [NotMapped]
    public IFormFile? ImageFile { get; set; }
    public byte[]? ImageData { get; set; }
    public string? ImageType { get; set; }

    // Status Flags
    [Display(Name = "Have Read?")]
    public bool HaveRead { get; set; }

    [Display(Name = "Wishlist?")]
    public bool IsWishlist { get; set; }

    // Foreign key for the user who owns this book
    [Required]
    public string? UserId { get; set; }

    // Navigation property
    [ForeignKey("UserId")]
    public virtual AppUser? User { get; set; }
    public virtual Category? Category { get; set; }

}
