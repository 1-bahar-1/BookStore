namespace BookStore.Models;

public class Keyword : BaseEntity
{
    public int Id { get; set; }

    public string Word { get; set; }

    public List<BookKeyword> BookKeywords { get; set; }
}