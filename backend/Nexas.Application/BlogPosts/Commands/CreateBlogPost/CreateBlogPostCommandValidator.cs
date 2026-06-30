using FluentValidation;

namespace Nexas.Application.BlogPosts.Commands.CreateBlogPost;

public class CreateBlogPostCommandValidator : AbstractValidator<CreateBlogPostCommand>
{
    public CreateBlogPostCommandValidator()
    {
        RuleFor(v => v.Title).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Subject).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Content).NotEmpty();
        RuleFor(v => v.Tags).MaximumLength(500);
        RuleFor(v => v.HeaderImageUrl).MaximumLength(500);
    }
}
