using FluentValidation;

namespace Nexas.Application.Purchases.Commands.RefundCourse;

public class RefundCourseCommandValidator : AbstractValidator<RefundCourseCommand>
{
    public RefundCourseCommandValidator()
    {
        RuleFor(x => x.PurchaseId).GreaterThan(0);
    }
}
