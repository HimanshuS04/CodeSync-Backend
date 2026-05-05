using CodeSync.AuthService.Data;
using CodeSync.AuthService.Interfaces;
using CodeSync.AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.AuthService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _context;

        public UserRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<User?> FindByEmailAsync(string email)
            => await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User?> FindByUserIdAsync(Guid userId)
            => await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

        public async Task<bool> ExistsByEmailAsync(string email)
            => await _context.Users.AnyAsync(u => u.Email == email);

        public async Task<bool> ExistsByUsernameAsync(string username)
            => await _context.Users.AnyAsync(u => u.Username == username);

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<List<User>> GetAllUsersAsync()
            => await _context.Users.ToListAsync();

        public async Task<List<User>> SearchByUsernameAsync(string query)
            => await _context.Users
                .Where(u => u.Username.ToLower()
                .Contains(query.ToLower()))
                .ToListAsync();
    }
}