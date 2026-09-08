namespace Nexas.Application.Users.Queries.GetUsers;

public class UserDto
{
    public int Id { get; set; }
    public string ExternalId { get; set; } = null!;
    public string? Name { get; set; }
    public string? Email { get; set; }
}
