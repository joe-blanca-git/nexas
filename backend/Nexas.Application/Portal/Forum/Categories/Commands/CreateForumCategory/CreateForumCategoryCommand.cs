using MediatR;

namespace Nexas.Application.Portal.Forum.Categories.Commands.CreateForumCategory;

public record CreateForumCategoryCommand(string Name, string? Description) : IRequest<int>;
