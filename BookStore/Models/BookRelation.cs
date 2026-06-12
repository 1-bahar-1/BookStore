namespace BookStore.Models;

public class BookRelation
{
    public int Id { get; set; }

    public int BookId { get; set; }
    public Book Book { get; set; }

    public int RelatedBookId { get; set; }
    public Book RelatedBook { get; set; }
}