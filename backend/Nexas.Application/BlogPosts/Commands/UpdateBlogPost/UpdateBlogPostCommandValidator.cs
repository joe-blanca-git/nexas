using FluentValidation;

namespace Nexas.Application.BlogPosts.Commands.UpdateBlogPost;

public class UpdateBlogPostCommandValidator : AbstractValidator<UpdateBlogPostCommand>
{
    public UpdateBlogPostCommandValidator()
    {
        RuleFor(v => v.Id).GreaterThan(0);
        RuleFor(v => v.Title).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Subject).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Content).NotEmpty();
        RuleFor(v => v.Tags).MaximumLength(500);
        RuleFor(v => v.HeaderImageUrl).MaximumLength(500);
    }
}
