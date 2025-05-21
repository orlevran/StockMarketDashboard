using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UsersMicroservice.Controllers;
using UsersMicroservice.Models;
using UsersMicroservice.Models.DTOs;
using UsersMicroservice.Services;

namespace UsesrsMicroservice.Tests.Controllers
{
    public class UserControllerTests
    {
        public static IEnumerable<object[]> UpdateUserTestCases_Valid =>
            new List<object[]>
            {
                new object[] { "123456123456123456123456", new Dictionary<string, string> { { "FirstName", "Skyler" }, { "LastName", "White" } } }
            };

        public static IEnumerable<object[]> UpdateUserTestCases_Invalid =>
            new List<object[]>
            {
                new object[] { "123456123456123456123456", new Dictionary<string, string> { { "_id", "654321654321654321654321" }, { "FirstName", "Skyler" }, { "LastName", "White" } } }
            };

        public static IEnumerable<object[]> DeleteUser => 
            new List<object[]>
            {
                new object[] { new Dictionary<string, string>{ {"id", "123456123456123456123456" } } }
            };
        /*
         * Arrange:

                A Mock<IUserService> is created to simulate the service behavior.

                RegisterUserAsync is set up to return true when called.

            Act:

                The RegisterUser method is called with a valid UserRegisterRequest.

            Assert:

                The result is checked to be of type OkObjectResult.

                The content of the response (Value) is verified to match the expected message.
         */

        #region Register
        // RegisterUser - Success
        [Fact]
        public async Task RegisterUser_ReturnsOk_WhenUserIsCreated()
        {
            // Arrange
            var mockService = new Mock<IUserService>();

            // Mocking the Service Method
            mockService.Setup(service => service.RegisterUserAsync(It.IsAny<UserRegisterRequest>()))
                       .ReturnsAsync(true);

            // Initialize the Controller with the Mocked Service
            var controller = new UserController(mockService.Object);

            // Prepare the DTO for the registration
            var newUser = new UserRegisterRequest
            {
                Email = "test@example.com",
                Password = "password123",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+123456789"
            };

            // Act - Call the Controller Method
            var result = await controller.RegisterUser(newUser);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be("User registered successfully");
        }

        // RegisterUser - Failure (User Already Exists)
        [Fact]
        public async Task RegisterUser_ReturnsBadRequest_WhenUserAlreadyExists()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(service => service.RegisterUserAsync(It.IsAny<UserRegisterRequest>()))
                       .ReturnsAsync(false);

            var controller = new UserController(mockService.Object);

            var newUser = new UserRegisterRequest
            {
                Email = "existing@example.com",
                Password = "password123",
                FirstName = "Jane",
                LastName = "Doe",
                PhoneNumber = "+123456789"
            };

            // Act
            var result = await controller.RegisterUser(newUser);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("User already exists or registration failed", badRequestResult.Value);
        }

        // RegisterUser - Failure (Request is null)
        [Fact]
        public async Task RegisterUser_ReturnsBadRequest_WhenRequestIsNull()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(service => service.RegisterUserAsync(It.IsAny<UserRegisterRequest>()))
                       .ReturnsAsync(false);

            var controller = new UserController(mockService.Object);

            UserRegisterRequest? newUser = null;

            // Act
            var result = await controller.RegisterUser(newUser);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("User data is missing", badRequestResult.Value);
        }
        #endregion

        #region Login
        // Login User - Success
        [Fact]
        public async Task LoginUser_ReturnsOK_WhenCredentialsAreValid()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(service => service.AuthenticateAsync("test@example.com", "password123"))
                .ReturnsAsync(new User { Email = "test@example.com" });

            // Initialize the Controller with the Mocked Service
            var controller = new UserController(mockService.Object);

            // Act
            var result = await controller.Login(new UserLoginRequest
            {
                Email = "test@example.com",
                Password = "password123"
            });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var user = okResult.Value as User;
            Assert.Equal("test@example.com", user?.Email);
        }

        // Login User - Failure (Invalid Credentials)
        [Fact]
        public async Task LoginUser_ReturnsUnauthorizedObjectResult_WhenCredentialsAreInvalid()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(service => service.AuthenticateAsync("test@example.com", "wrong password"))
                .ReturnsAsync((User?)null);

            // Initialize the Controller with the Mocked Service
            var controller = new UserController(mockService.Object);

            // Act
            var result = await controller.Login(new UserLoginRequest
            {
                Email = "test@example.com",
                Password = "wrong password"
            });

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
        }
        #endregion

        #region Search
        // Search Users - Found by email
        [Fact]
        public async Task SearchUser_ReturnsUser_WhenFoundByEmail()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(service => service.GetUserByIdentifierAsync("Email", "test@example.com"))
                .ReturnsAsync(new User { Email = "test@example.com", FirstName = "John" });

            // Initialize the controller
            var controller = new UserController(mockService.Object);

            // Act
            var result = await controller.SearchUsers(null, "test@example.com");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var user = okResult.Value as User;
            Assert.Equal("John", user?.FirstName);
        }

        // Search Users - Not Found (Returns BadRequest If Email and Id Are Both Null)
        [Fact]
        public async Task SearchUser_ReturnsBadRequest_WhenIdAndEmailAreNull()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(service => service.GetUserByIdentifierAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            // Initialize the controller
            var controller = new UserController(mockService.Object);

            // Act
            var result = await controller.SearchUsers(null, null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("You must provide either an Id or an Email to search.", badRequestResult.Value);
        }

        // Search User - Not found (Return NotFound if user doesn't exist)
        [Fact]
        public async Task SearchUser_ReturnsBadRequest_WhenUserDoesNotExist()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(service => service.GetUserByIdentifierAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            // Initialize the controller
            var controller = new UserController(mockService.Object);

            // Act
            var result = await controller.SearchUsers(null, "doesntExist@example.com");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        }
        #endregion

        #region Edit
        // Edit User - Success
        [Theory]
        [MemberData(nameof(UpdateUserTestCases_Valid))]
        public async Task EditUser_ReturnsOk_WhereUserIsUpdated(string id, Dictionary<string, string>? updates)
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(service => service.UpdateUserFieldsAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(true);

            // Initialize the controller
            var controller = new UserController(mockService.Object);

            // Act
            var result = await controller.UpdateUser(id, updates);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("User updated successfully.", okResult.Value);
        }

        // Edit User - Failure (Returns BadRequest If Updates Dictionary is Null)
        [Theory]
        [InlineData("123456123456123456123456", null)]
        public async Task EditUser_ReturnsBadRequest_WhenUpdatesAreNull(string id, Dictionary<string, string>? updates)
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(service => service.UpdateUserFieldsAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(false);

            // Initialize the controller
            var controller = new UserController(mockService.Object);

            // Act
            var result = await controller.UpdateUser(id, updates);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No fields provided for update.", badRequestResult.Value);
        }

        // Edit User - Failure (Returns BadRequest If Update Dictionary contains _id)
        [Theory]
        [MemberData(nameof(UpdateUserTestCases_Invalid))]
        public async Task EditUser_ReturnsBadRequest_WhenUpdatesContainId(string id, Dictionary<string, string>? updates)
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(service => service.UpdateUserFieldsAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(false);

            // Initialize the controller
            var controller = new UserController(mockService.Object);

            // Act
            var result = await controller.UpdateUser(id, updates);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Cannot update the _id field.", badRequestResult.Value);
        }

        // Edit User - Failure (Returns NotFound If User Doesn't Exist)
        [Theory]
        [MemberData(nameof(UpdateUserTestCases_Valid))]
        public async Task EditUser_ReturnsNotFound_WhenUserDoesNotExist(string id, Dictionary<string, string>? updates)
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(service => service.UpdateUserFieldsAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(false);

            // Initialize the controller
            var controller = new UserController(mockService.Object);

            // Act
            var result = await controller.UpdateUser(id, updates);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found.", notFoundResult.Value);
        }

        #endregion

        #region Delete

        // Delete User - Success
        [Theory]
        [MemberData(nameof(DeleteUser))]
        public async Task DeleteUser_ReturnsOk_WhenUserIsDeleted(Dictionary<string, string> requestBody)
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(service => service.DeleteUserAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Initialize the controller
            var controller = new UserController(mockService.Object);

            // Act
            var result = await controller.DeleteUser(requestBody);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("User deleted successfully.", okResult.Value);
        }

        // Delete User - Failure (User Doesn't exist)
        [Theory]
        [MemberData(nameof(DeleteUser))]
        public async Task DeleteUser_ReturnsNotFound_WhenUserDoesNotExist(Dictionary<string, string> requestBody)
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(service => service.DeleteUserAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            // Initialize the controller
            var controller = new UserController(mockService.Object);

            // Act
            var result = await controller.DeleteUser(requestBody);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found.", notFoundResult.Value);
        }

        #endregion
    }
}
