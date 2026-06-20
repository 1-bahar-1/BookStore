namespace BookStore.Models;

public class Category : BaseEntity
{
    public int Id { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(100)]
    public string Slug { get; set; } = string.Empty;

    public ICollection<Book> Books { get; set; } = new List<Book>();
}