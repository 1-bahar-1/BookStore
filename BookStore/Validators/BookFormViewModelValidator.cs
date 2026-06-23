using BookStore.Areas.Admin.Models;
using FluentValidation;

namespace BookStore.Validators;

public class BookFormViewModelValidator : AbstractValidator<BookFormViewModel>
{
    public BookFormViewModelValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان کتاب الزامی است")
            .MaximumLength(300).WithMessage("عنوان نمی‌تواند بیشتر از 300 کاراکتر باشد");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug الزامی است")
            .MaximumLength(20).WithMessage("Slug نمی‌تواند بیشتر از 20 کاراکتر باشد");

        RuleFor(x => x.PageCount)
            .GreaterThan(0).WithMessage("تعداد صفحات باید بزرگتر از صفر باشد")
            .LessThanOrEqualTo(10000).WithMessage("تعداد صفحات نمی‌تواند بیشتر از 10000 باشد");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("دسته‌بندی الزامی است");

        RuleFor(x => x.ISBN)
            .MaximumLength(20).WithMessage("ISBN نمی‌تواند بیشتر از 20 کاراکتر باشد")
            .When(x => !string.IsNullOrEmpty(x.ISBN));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("توضیحات نمی‌تواند بیشتر از 2000 کاراکتر باشد")
            .When(x => !string.IsNullOrEmpty(x.Description));


    }
}
