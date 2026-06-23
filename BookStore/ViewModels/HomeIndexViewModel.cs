using System;

namespace BookStore.ViewModels;

public class HomeIndexViewModel
{
    public List<BookCardDto> Books { get; set; } = new();
    public List<BookStore.Models.Category> Categories { get; set; } = new();
    public List<BookStore.Models.Author> Authors { get; set; } = new();
    public List<BookStore.Models.Keyword> Keywords { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
    public int? SelectedCategoryId { get; set; }
    public int? SelectedAuthorId { get; set; }
    public int? SelectedKeywordId { get; set; }
    public string? SearchQuery { get; set; }
}

public class BookCardDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public int PageCount { get; set; }
    public bool IsFree { get; set; }
    public string? CoverImagePath { get; set; }
    public string? CategoryName { get; set; }
}
