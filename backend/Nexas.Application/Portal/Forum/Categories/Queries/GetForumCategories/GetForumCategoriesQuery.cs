using MediatR;

namespace Nexas.Application.Portal.Forum.Categories.Queries.GetForumCategories;

public record GetForumCategoriesQuery() : IRequest<List<ForumCategoryDto>>;
