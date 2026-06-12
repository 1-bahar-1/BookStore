namespace BookStore.Models;

public class Author : BaseEntity
{
    public int Id { get; set; }

    public string FullName { get; set; }
    public string Slug { get; set; }

    public List<BookAuthor> BookAuthors { get; set; }
}