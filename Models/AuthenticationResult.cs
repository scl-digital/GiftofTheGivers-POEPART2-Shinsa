namespace DisasterAlleviationFoundation.Models
{
    public class AuthenticationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public User? User { get; set; }
        public string? Token { get; set; }
        
        public static AuthenticationResult Success(User user, string? token = null)
        {
            return new AuthenticationResult
            {
                IsSuccess = true,
                User = user,
                Token = token,
                Message = "Authentication successful"
            };
        }
        
        public static AuthenticationResult Failure(string message)
        {
            return new AuthenticationResult
            {
                IsSuccess = false,
                Message = message
            };
        }
    }
}