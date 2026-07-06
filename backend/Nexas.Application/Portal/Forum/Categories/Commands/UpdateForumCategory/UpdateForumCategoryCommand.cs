using MediatR;

namespace Nexas.Application.Portal.Forum.Categories.Commands.UpdateForumCategory;

public record UpdateForumCategoryCommand(int Id, string Name, string? Description) : IRequest;
