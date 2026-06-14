namespace BookStore.Models.ViewModels;

public class BookFilterViewModel
{
    public IEnumerable<Book> Books { get; set; } = new List<Book>();
    public IEnumerable<Category> Categories { get; set; } = new List<Category>();
    public IEnumerable<Author> Authors { get; set; } = new List<Author>();
    public IEnumerable<Keyword> Keywords { get; set; } = new List<Keyword>();

    public int? CategoryId { get; set; }
    public int? AuthorId { get; set; }
    public int? KeywordId { get; set; }
    public string? Search { get; set; }

    public int Page { get; set; } = 1;
    public int TotalPages { get; set; }
}

