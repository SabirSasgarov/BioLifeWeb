using BioLife.Domain.Entity;
using BioLife.MVC.Models;
using Microsoft.AspNetCore.Identity;

namespace BioLife.MVC.Services
{
    public interface IAccountService
    {
        Task<AppUser?> GetUserByEmailAsync(string email);
        Task<AppUser?> GetUserByIdAsync(string id);
        Task<IList<string>> GetUserRolesAsync(AppUser user);
        Task<SignInResult> PasswordSignInAsync(string email, string password, bool rememberMe);
        Task SignOutAsync();
        Task<IdentityResult> CreateUserAsync(AppUser user, string password);
        Task<bool> RoleExistsAsync(string roleName);
        Task CreateRoleAsync(string roleName);
        Task AddToRoleAsync(AppUser user, string roleName);
        Task<string> GenerateEmailConfirmationTokenAsync(AppUser user);
        Task<IdentityResult> ConfirmEmailAsync(AppUser user, string token);
        Task RequestPasswordResetAsync(string email, string linkTemplate);
        Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword);
    }
}