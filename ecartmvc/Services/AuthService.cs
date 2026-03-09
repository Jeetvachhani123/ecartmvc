using ecartmvc.Data;
using ecartmvc.Models;
using Microsoft.EntityFrameworkCore;

namespace ecartmvc.Services
{
    public class AuthService
    {
        private readonly EcartDbContext _context;

        public AuthService(EcartDbContext context)
        {
            _context = context;
        }

        // Login Authentication
        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
        }

        // Check if user already exists by username or email
        public User? GetUserByUsernameOrEmail(string username, string email)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username || u.Email == email);
        }

        // Register a new user
        public void RegisterUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }
    }
}
