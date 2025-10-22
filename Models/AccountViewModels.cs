namespace ContractClaimSystem.Models
{
    // ViewModel for user registration
    public class RegisterViewModel
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }
    }

    // ViewModel for initiating password reset
    public class ForgotPasswordViewModel
    {
        public string Email { get; set; }
    }

    // ViewModel for completing password reset
    public class ResetPasswordViewModel
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public string Token { get; set; }
    }
}
