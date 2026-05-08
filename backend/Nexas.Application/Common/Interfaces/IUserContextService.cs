using Nexas.Domain.Entities;

namespace Nexas.Application.Common.Interfaces
{
    public interface IUserContextService
    {
        Task<User> GetCurrentUserAsync();
    }
}
