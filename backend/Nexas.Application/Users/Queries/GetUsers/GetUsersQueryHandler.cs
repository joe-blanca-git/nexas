using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Users.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private readonly INexasDbContext _context;

    public GetUsersQueryHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .OrderBy(u => u.FullName)
            .Select(u => new UserDto
            {
                Id = u.Id,
                ExternalId = u.ExternalId,
                Name = u.FullName,
                Email = u.Email
            })
            .ToListAsync(cancellationToken);

        return users;
    }
}
