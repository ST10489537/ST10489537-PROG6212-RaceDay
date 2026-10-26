using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    /// <summary>Fields an Organiser supplies to create or update a category.</summary>
    public class CategoryRequestDto
    {
        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Description { get; set; }
    }

    /// <summary>A category as returned by the API.</summary>
    public class CategoryDto
    {
        public int CategoryID { get; set; }
        public int EventID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}