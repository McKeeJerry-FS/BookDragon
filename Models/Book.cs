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
  
}
