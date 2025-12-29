using MarketPlacer.DAL.Models;

namespace MarketPlacer.DAL.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetUserByEmailAsync(string email);
}