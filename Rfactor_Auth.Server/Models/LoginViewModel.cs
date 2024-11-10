namespace Rfactor_Auth.Server.Models
{
    public class LoginViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RefreshRequest
    {
        public string RefreshToken { get; set; }
    }

    public class RefreshToken
    {
        public string Token { get; set; }
        public string Username { get; set; }
    }

    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string Fio { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

}
