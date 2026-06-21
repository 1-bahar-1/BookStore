using System.ComponentModel.DataAnnotations;

namespace BookStore.Models;

public class Keyword : BaseEntity
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Word { get; set; } = string.Empty;

    public ICollection<BookKeyword> BookKeywords { get; set; } = new List<BookKeyword>();
}