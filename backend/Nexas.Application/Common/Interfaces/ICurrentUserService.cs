namespace Nexas.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        string? ExternalId { get; }
        string? Email { get; }
    }
}
