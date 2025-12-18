using Microsoft.EntityFrameworkCore;
using MarketPlacer.DAL.Models;

namespace MarketPlacer.DAL.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        // Aqui usamos o _dbSet que herdamos do GenericRepository
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }
}