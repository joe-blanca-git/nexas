using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;

namespace Nexas.Application.Common.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly INexasDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public UserContextService(INexasDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<User> GetCurrentUserAsync()
        {
            var externalId = _currentUserService.ExternalId;

            if (string.IsNullOrEmpty(externalId))
            {
                throw new UnauthorizedAccessException("User is not authenticated or external ID is missing in token.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.ExternalId == externalId);

            if (user == null)
            {
                var email = _currentUserService.Email ?? "unknown@nexas.com"; // Default if missing
                user = User.Create(externalId, email);
                _context.Users.Add(user);
                await _context.SaveChangesAsync(default);
            }

            return user;
        }
    }
}
