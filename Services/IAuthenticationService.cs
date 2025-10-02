using DisasterAlleviationFoundation.Models;

namespace DisasterAlleviationFoundation.Services
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> LoginAsync(LoginViewModel request);
        Task<AuthenticationResult> RegisterAsync(RegisterViewModel request);
        Task<bool> ValidateUserAsync(string email, string password);
        User? GetCurrentUser();
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int id);
        Task<List<User>> GetAllUsersAsync();
        bool IsAuthenticated { get; }
    }
}