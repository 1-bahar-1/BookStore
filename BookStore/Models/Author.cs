namespace BookStore.Models;

public class Author : BaseEntity
{
    public int Id { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
}