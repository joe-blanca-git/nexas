namespace Nexas.Application.ForumCategories.Queries;

public record ForumCategoryDto(int Id, string Name, string? Description, bool Active, string? Icon);
