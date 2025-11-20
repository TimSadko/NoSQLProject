using NoSQLProject.Models;
using NoSQLProject.Other;
using NoSQLProject.Repositories;

namespace NoSQLProject.Services
{
    public class PasswordResetService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public PasswordResetService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public string GenerateTokenForUser(Employee user)
        {
            return Hasher.GetHashedString(user.Id + user.Password);
        }

        public async Task<string?> GenerateTokenForEmailAsync(string email)
        {
            var user = await _employeeRepository.GetByEmailAsync(email);
            if (user == null) return null;
            return GenerateTokenForUser(user);
        }

        public async Task<bool> ValidateTokenAsync(string userId, string token)
        {
            var user = await _employeeRepository.GetByIdAsync(userId);
            if (user == null) return false;
            var expected = GenerateTokenForUser(user);
            return token == expected;
        }

        public async Task<bool> ResetPasswordAsync(string userId, string token, string newPassword)
        {
            var user = await _employeeRepository.GetByIdAsync(userId);
            if (user == null) return false;
            var expected = GenerateTokenForUser(user);
            if (token != expected) return false;

            user.Password = Hasher.GetHashedString(newPassword);
            await _employeeRepository.UpdateAsync(user);
            return true;
        }
    }
}