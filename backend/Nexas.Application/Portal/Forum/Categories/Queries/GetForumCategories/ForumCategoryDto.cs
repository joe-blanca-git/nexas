namespace Nexas.Application.Portal.Forum.Categories.Queries.GetForumCategories;

public class ForumCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Active { get; set; }
}
