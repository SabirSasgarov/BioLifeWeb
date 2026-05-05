using BioLife.Application;
using BioLife.Application.Common.Interfaces;
using BioLife.Domain.Entity;
using BioLife.MVC.Models;
using Microsoft.AspNetCore.Identity;

namespace BioLife.MVC.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IEmailService _emailService;

        public AccountService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<AppRole> roleManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailService = emailService;
        }

        public async Task<AppUser?> GetUserByEmailAsync(string email) => await _userManager.FindByEmailAsync(email);

        public async Task<AppUser?> GetUserByIdAsync(string id) => await _userManager.FindByIdAsync(id);

        public async Task<IList<string>> GetUserRolesAsync(AppUser user) => await _userManager.GetRolesAsync(user);

        public async Task<SignInResult> PasswordSignInAsync(string email, string password, bool rememberMe)
        {
            return await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);
        }

        public async Task SignOutAsync() => await _signInManager.SignOutAsync();

        public async Task<IdentityResult> CreateUserAsync(AppUser user, string password) => await _userManager.CreateAsync(user, password);

        public async Task<bool> RoleExistsAsync(string roleName) => await _roleManager.RoleExistsAsync(roleName);

        public async Task CreateRoleAsync(string roleName) => await _roleManager.CreateAsync(new AppRole { Name = roleName });

        public async Task AddToRoleAsync(AppUser user, string roleName) => await _userManager.AddToRoleAsync(user, roleName);

        public async Task<string> GenerateEmailConfirmationTokenAsync(AppUser user) => await _userManager.GenerateEmailConfirmationTokenAsync(user);

        public async Task<IdentityResult> ConfirmEmailAsync(AppUser user, string token)
        {
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(user);
            }
            return result;
        }

        public async Task RequestPasswordResetAsync(string email, string linkTemplate)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user))) return;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            string link = linkTemplate.Replace("{{Token}}", token);

            await _emailService.SendEmailAsync(user.Email, "Reset Password", $"Please reset your password by clicking here: <a href='{link}'>link</a>");
        }

        public async Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }
    }
}