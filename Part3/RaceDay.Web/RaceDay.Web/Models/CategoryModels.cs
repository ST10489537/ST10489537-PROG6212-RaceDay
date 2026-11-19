using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

// A category as returned by the Part 2 API.
public class CategoryDto
{
    public int CategoryID { get; set; }
    public int EventID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

// Fields an Organiser fills in to add or edit a category.
public class CategoryRequestDto
{
    [Required(ErrorMessage = "Please enter a category name.")]
    [MaxLength(50, ErrorMessage = "The name can be at most 50 characters.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300, ErrorMessage = "The description can be at most 300 characters.")]
    public string? Description { get; set; }
}