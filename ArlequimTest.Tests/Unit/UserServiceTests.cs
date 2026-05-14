using ArlequimTest.Api.DTOs;
using ArlequimTest.Api.Exceptions;
using ArlequimTest.Api.Services;

namespace ArlequimTest.Tests.Unit
{
    public class UserServiceTests
    {
        private readonly UserService _service = new UserService();

        [Fact]
        public void Create_ShouldReturnUser_WhenDataIsValid()
        {
            var dto = new CreateUserDto
            {
                Name = "UnitCreateTest",
                Email = "UnitCreateTest@email.com",
                Password = "123456",
                Role = "Seller"
            };

            var result = _service.Create(dto);

            Assert.Equal("UnitCreateTest", result.Name);
            Assert.Equal("UnitCreateTest@email.com", result.Email);
        }

        [Fact]
        public void Create_ShouldThrow_WhenEmailAlreadyUsed()
        {

            var newUser = new CreateUserDto
            {
                Name = "UnitUsedMailErrorTest1",
                Email = "UnitUsedMail@email.com",
                Password = "123456",
                Role = "Seller"
            };

            _service.Create(newUser);

            var dto = new CreateUserDto
            {
                Name = "UnitUsedMailErrorTest2",
                Email = "unitusedmail@email.com",
                Password = "123456",
                Role = "Admin"
            };

            var exception = Assert.Throws<ValidationError>(() => _service.Create(dto));
            Assert.Equal("Email already in use", exception.Message);
        }

        [Fact]
        public void Create_ShouldThrow_WhenPasswordTooShort()
        {

            var dto = new CreateUserDto
            {
                Name = "UnitShortPassErrorTest",
                Email = "UnitShortPassErrorTest@email.com",
                Password = "123",
                Role = "Admin"
            };

            var exception = Assert.Throws<ValidationError>(() => _service.Create(dto));
            Assert.Equal("Password do not attend the minimal security requisites(6 characters)", exception.Message);
        }
    }
}
