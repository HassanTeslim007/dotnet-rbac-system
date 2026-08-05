using Microsoft.EntityFrameworkCore;
using RbacSystem.Application.Interfaces.Repositories;
using RbacSystem.Domain.Entities;
using RbacSystem.Infrastructure.Persistence;

namespace RbacSystem.Infrastructure.Repositories;

public class RoleRepository(AppDbContext context) : IRoleRepository
{
    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        => await context.Roles.FirstOrDefaultAsync(r => r.Name == name, cancellationToken);

    public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Roles.ToListAsync(cancellationToken);

    public async Task AddAsync(Role role, CancellationToken cancellationToken = default)
        => await context.Roles.AddAsync(role, cancellationToken);

    public Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        context.Roles.Update(role);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await GetByIdAsync(id, cancellationToken);
        if (role is not null) context.Roles.Remove(role);
    }
}
