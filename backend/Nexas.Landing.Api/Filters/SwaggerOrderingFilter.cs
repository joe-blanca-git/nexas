using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Nexas.Landing.Api.Filters;

public class SwaggerOrderingFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var order = new[] { "Courses", "Teachers", "BlogPosts", "Purchases", "Subscriptions" };

        if (swaggerDoc.Tags == null)
        {
            swaggerDoc.Tags = new List<OpenApiTag>();
        }

        var uniqueTags = context.ApiDescriptions
            .SelectMany(desc => desc.ActionDescriptor.EndpointMetadata.OfType<Microsoft.AspNetCore.Http.Metadata.ITagsMetadata>())
            .SelectMany(t => t.Tags)
            .Distinct();

        foreach (var tag in uniqueTags)
        {
            if (!swaggerDoc.Tags.Any(t => t.Name == tag))
            {
                swaggerDoc.Tags.Add(new OpenApiTag { Name = tag });
            }
        }

        swaggerDoc.Tags = swaggerDoc.Tags
            .OrderBy(tag =>
            {
                var index = Array.IndexOf(order, tag.Name);
                return index == -1 ? int.MaxValue : index;
            })
            .ThenBy(tag => tag.Name)
            .ToList();
    }
}
