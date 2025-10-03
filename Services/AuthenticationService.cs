using System.Security.Cryptography;
using System.Text.Json;
using DisasterAlleviationFoundation.Models;

namespace DisasterAlleviationFoundation.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly List<User> _users = new();
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _dataDirectory;
        private readonly string _usersFilePath;
        
        public AuthenticationService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            // Set data directory (local to app)
            _dataDirectory = Path.Combine(AppContext.BaseDirectory, "App_Data");
            _usersFilePath = Path.Combine(_dataDirectory, "users.json");

            // Ensure data dir exists
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }

            // Load existing users from disk; if none, seed demo and save
            LoadUsers();
            if (_users.Count == 0)
            {
                InitializeSampleData();
                SaveUsers();
            }
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
                
                // Persist last login update (optional)
                SaveUsers();

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
                    Role = "User",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _users.Add(user);

                // Set session
                _httpContextAccessor.HttpContext?.Session.SetInt32("UserId", user.Id);
                _httpContextAccessor.HttpContext?.Session.SetString("UserEmail", user.Email);
                _httpContextAccessor.HttpContext?.Session.SetString("UserName", user.FullName);

                // Persist to disk
                SaveUsers();

                return AuthenticationResult.Success(user);
            }
            catch (Exception ex)
            {
                return AuthenticationResult.Failure($"Registration failed: {ex.Message}");
            }
        }

        private void InitializeSampleData()
        {
            // Single demo user for testing
            var demoUser = new User
            {
                Id = 1,
                FirstName = "Demo",
                LastName = "User",
                Email = "demo@daf.org",
                PasswordHash = HashPassword("demo123"),
                Role = "User",
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                IsActive = true
            };

            _users.Add(demoUser);
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

        // Persistence helpers
        private void LoadUsers()
        {
            try
            {
                if (File.Exists(_usersFilePath))
                {
                    var json = File.ReadAllText(_usersFilePath);
                    var users = JsonSerializer.Deserialize<List<User>>(json);
                    if (users != null)
                    {
                        _users.Clear();
                        _users.AddRange(users);
                    }
                }
            }
            catch
            {
                // Ignore read errors; start with an empty list
            }
        }

        private void SaveUsers()
        {
            try
            {
                var json = JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_usersFilePath, json);
            }
            catch
            {
                // Ignore write errors to avoid breaking the app flow
            }
        }

        // Interface implementations
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
    }
}