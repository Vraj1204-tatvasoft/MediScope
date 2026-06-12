namespace MediScope.Business.Services.Interfaces
{
    public interface IPasswordResetService
    {
        Task ForgotPasswordAsync(string email);
        Task<bool> ValidateTokenAsync(string token);
        Task ResetPasswordAsync(string token, string newPassword);
    }
}