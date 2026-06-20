using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Areas.Admin.Models;

public class BookFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "عنوان کتاب الزامی است")]
    [MaxLength(300, ErrorMessage = "عنوان نمی‌تواند بیشتر از 300 کاراکتر باشد")]
    public string? Title { get; set; }

    [Required(ErrorMessage = "Slug الزامی است")]
    [MaxLength(300, ErrorMessage = "Slug نمی‌تواند بیشتر از 300 کاراکتر باشد")]
    public string? Slug { get; set; }

    [Range(1, 10000, ErrorMessage = "تعداد صفحات باید بین 1 و 10000 باشد")]
    public int PageCount { get; set; }

    [Required(ErrorMessage = "دسته‌بندی الزامی است")]
    public int CategoryId { get; set; }

    public string? FilePath { get; set; }

    [MaxLength(20, ErrorMessage = "ISBN نمی‌تواند بیشتر از 20 کاراکتر باشد")]
    public string? ISBN { get; set; }

    public bool IsFree { get; set; } = true;

    [MaxLength(2000, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 2000 کاراکتر باشد")]
    public string? Description { get; set; }

    [MaxLength(500, ErrorMessage = "مسیر کاور نمی‌تواند بیشتر از 500 کاراکتر باشد")]
    public string? CoverImagePath { get; set; }

    public int? PublishedYear { get; set; }

    public int[]? AuthorIds { get; set; }
    public int[]? KeywordIds { get; set; }
    public int[]? RelatedBookIds { get; set; }

    public List<BookStore.Models.Category>? Categories { get; set; }
    public List<AuthorSelection> Authors { get; set; } = new List<AuthorSelection>();
    public List<KeywordSelection> Keywords { get; set; } = new List<KeywordSelection>();
    public List<RelatedBookSelection> RelatedBooks { get; set; } = new List<RelatedBookSelection>();
}

public class AuthorSelection
{
    public int AuthorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}

public class KeywordSelection
{
    public int KeywordId { get; set; }
    public string Word { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}

public class RelatedBookSelection
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}
