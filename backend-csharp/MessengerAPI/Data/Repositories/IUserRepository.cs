using MessengerAPI.Models;

namespace MessengerAPI.Data.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> SearchAsync(string query);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
}
