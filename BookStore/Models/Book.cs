namespace BookStore.Models;

public class Book
{
    public int Id { get; set; }

    public string Title { get; set; }
    public string Slug { get; set; }
    public string FilePath { get; set; }
    public int PageCount { get; set; }

    
    public int CategoryId { get; set; }
    public Category Category { get; set; }

    
    public List<BookAuthor> BookAuthors { get; set; }
    public List<BookKeyword> BookKeywords { get; set; }

    public List<BookRelation> RelatedBooks { get; set; }
}