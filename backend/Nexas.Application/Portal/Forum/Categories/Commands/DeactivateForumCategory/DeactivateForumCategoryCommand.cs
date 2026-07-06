using MediatR;

namespace Nexas.Application.Portal.Forum.Categories.Commands.DeactivateForumCategory;

public record DeactivateForumCategoryCommand(int Id) : IRequest;
