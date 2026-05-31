using MediatR;

namespace Nexas.Application.Purchases.Commands.RefundCourse;

public record RefundCourseCommand(int PurchaseId) : IRequest<RefundCourseResponseDto>;
