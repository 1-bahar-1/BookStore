using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models;

public class Book : BaseEntity
{
    public Book()
    {
        BookAuthors = new List<BookAuthor>();
        BookKeywords = new List<BookKeyword>();
        RelatedTo = new List<BookRelation>();
        RelatedFrom = new List<BookRelation>();
    }

    public int Id { get; set; }

    [Required(ErrorMessage = "عنوان کتاب الزامی است")]
    [MaxLength(300, ErrorMessage = "عنوان نمی‌تواند بیشتر از 300 کاراکتر باشد")]
    public string? Title { get; set; }

    [Required(ErrorMessage = "شناسه slug الزامی است")]
    [MaxLength(300, ErrorMessage = "Slug نمی‌تواند بیشتر از 300 کاراکتر باشد")]
    public string? Slug { get; set; }

    [MaxLength(20, ErrorMessage = "ISBN نمی‌تواند بیشتر از 20 کاراکتر باشد")]
    public string? ISBN { get; set; }

    public bool IsFree { get; set; } = true;

    [MaxLength(2000, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 2000 کاراکتر باشد")]
    public string? Description { get; set; }

    [MaxLength(500, ErrorMessage = "مسیر عکس نمی‌تواند بیشتر از 500 کاراکتر باشد")]
    public string? CoverImagePath { get; set; }

    public int? PublishedYear { get; set; }

    [MaxLength(500, ErrorMessage = "مسیر فایل نمی‌تواند بیشتر از 500 کاراکتر باشد")]
    public string? FilePath { get; set; }

    [Range(1, 10000, ErrorMessage = "تعداد صفحات باید بین 1 و 10000 باشد")]
    public int PageCount { get; set; }

    [Required(ErrorMessage = "دسته‌بندی الزامی است")]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public List<BookAuthor> BookAuthors { get; set; }
    public List<BookKeyword> BookKeywords { get; set; }
    public List<BookRelation> RelatedTo { get; set; }
    public List<BookRelation> RelatedFrom { get; set; }
}