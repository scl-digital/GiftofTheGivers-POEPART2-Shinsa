using System.Security.Cryptography;
using DisasterAlleviationFoundation.Models;

namespace DisasterAlleviationFoundation.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly List<User> _users = new();
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public AuthenticationService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            
            // Initialize with some sample data for demonstration
            InitializeSampleData();
        }
        
        public bool IsAuthenticated => GetCurrentUser() != null;
        
        public User? GetCurrentUser()
        {
            var userId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                return _users.FirstOrDefault(u => u.Id == userId.Value);
            }
            return null;
        }

        public async Task<AuthenticationResult> LoginAsync(LoginViewModel request)
        {
            try
            {
                var user = _users.FirstOrDefault(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));
                
                if (user == null)
                {
                    return AuthenticationResult.Failure("Invalid email or password");
                }
                
                if (!user.IsActive)
                {
                    return AuthenticationResult.Failure("Account is deactivated");
                }
                
                if (!VerifyPassword(request.Password, user.PasswordHash))
                {
                    return AuthenticationResult.Failure("Invalid email or password");
                }
                
                user.LastLoginAt = DateTime.UtcNow;
                
                // Set session
                _httpContextAccessor.HttpContext?.Session.SetInt32("UserId", user.Id);
                _httpContextAccessor.HttpContext?.Session.SetString("UserEmail", user.Email);
                _httpContextAccessor.HttpContext?.Session.SetString("UserName", user.FullName);
                
                return AuthenticationResult.Success(user);
            }
            catch (Exception ex)
            {
                return AuthenticationResult.Failure($"Login failed: {ex.Message}");
            }
        }

        public async Task<AuthenticationResult> RegisterAsync(RegisterViewModel request)
        {
            try
            {
                if (_users.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    return AuthenticationResult.Failure("Email already exists");
                }
                
                var user = new User
                {
                    Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    PasswordHash = HashPassword(request.Password),
                    Role = request.Role,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                
                _users.Add(user);
                
                // Set session
                _httpContextAccessor.HttpContext?.Session.SetInt32("UserId", user.Id);
                _httpContextAccessor.HttpContext?.Session.SetString("UserEmail", user.Email);
                _httpContextAccessor.HttpContext?.Session.SetString("UserName", user.FullName);
                
                return AuthenticationResult.Success(user);
            }
            catch (Exception ex)
            {
                return AuthenticationResult.Failure($"Registration failed: {ex.Message}");
            }
        }

        public async Task<bool> ValidateUserAsync(string email, string password)
        {
            var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            return user != null && VerifyPassword(password, user.PasswordHash);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return _users.ToList();
        }

        private void InitializeSampleData()
        {
            // Add a sample admin user for testing
            var adminUser = new User
            {
                Id = 1,
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@daf.org",
                PasswordHash = HashPassword("admin123"),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                IsActive = true
            };
            
            var volunteerUser = new User
            {
                Id = 2,
                FirstName = "John",
                LastName = "Volunteer",
                Email = "volunteer@daf.org",
                PasswordHash = HashPassword("volunteer123"),
                Role = "Volunteer",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                IsActive = true
            };
            
            _users.AddRange(new[] { adminUser, volunteerUser });
        }

        private string HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[32];
            rng.GetBytes(salt);
            
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);
            
            var hashBytes = new byte[64];
            Array.Copy(salt, 0, hashBytes, 0, 32);
            Array.Copy(hash, 0, hashBytes, 32, 32);
            
            return Convert.ToBase64String(hashBytes);
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            try
            {
                var hashBytes = Convert.FromBase64String(passwordHash);
                var salt = new byte[32];
                Array.Copy(hashBytes, 0, salt, 0, 32);
                
                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
                var hash = pbkdf2.GetBytes(32);
                
                for (int i = 0; i < 32; i++)
                {
                    if (hashBytes[i + 32] != hash[i])
                        return false;
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}