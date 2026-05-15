using ArlequimTest.Api.DTOs;
using ArlequimTest.Api.Models;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using ArlequimTest.Api.Exceptions;

namespace ArlequimTest.Api.Services
{
    public class UserService
    {
        private static List<User> _users = new();

        public User Create(CreateUserDto dto)
        {
            if (_users.Any(u => u.Email.ToLower() == dto.Email.ToLower()))
                throw new ConflictError("Email already in use");

            if (dto.Password.Length < 6)
                throw new ValidationError("Password do not attend the minimal security requisites(6 characters)");

            var user = new User
            {
                Id = _users.Count + 1,
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password), 
                Role = dto.Role
            };

            _users.Add(user);
            return user;
        }

        public string Login(string email, string password)
        {
            var user = _users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                throw new UnauthorizedError("Invalid Credentials");

            return GenerateToken(user);
        }

        private string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("chave-secreta-super-segura-aqui-32chars!"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: new[] {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
                },
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
