using CodeSync.AuthService.Models;

namespace CodeSync.AuthService.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> FindByEmailAsync(string email);
        Task<User?> FindByUserIdAsync(Guid userId);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByUsernameAsync(string username);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<List<User>> GetAllUsersAsync();
        Task<List<User>> SearchByUsernameAsync(string query);
    }
}