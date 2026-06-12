using Microsoft.EntityFrameworkCore;
using MediScope.Data;
using MediScope.Common.Models;
using MediScope.Common.Models.DTOs.Auth;
using MediScope.Common.Models.Entities;
using MediScope.Common.Models.Enums;
using MediScope.Business.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace MediScope.Business.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
        Task RevokeTokenAsync(string refreshToken);
    }
}