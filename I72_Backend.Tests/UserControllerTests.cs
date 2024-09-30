using Xunit;
using Moq;
using I72_Backend.Interfaces;
using I72_Backend.Model;
using I72_Backend.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json;

namespace I72_Backend.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<ILogger<UserController>> _loggerMock;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            // Set up the mock objects
            _userRepositoryMock = new Mock<IUserRepository>();
            _tokenServiceMock = new Mock<ITokenService>();
            _loggerMock = new Mock<ILogger<UserController>>();

            // Create the UserController instance with the mocked dependencies
            _controller = new UserController(_userRepositoryMock.Object, _tokenServiceMock.Object, _loggerMock.Object);
        }

        // Test Method for GetUsers
        [Fact] // This attribute indicates that this method is a test case
        public void GetUsers_ReturnsOkResult_WithUserList()
        {
            // Arrange
            // Creating a sample list of users to be returned by the mock repository
            var userList = new List<User>
            {
                new User { Id = 1, Username = "user1", FirstName = "User", LastName = "One", Phone = "123456", Role = "Admin" },
                new User { Id = 2, Username = "user2", FirstName = "User", LastName = "Two", Phone = "123457", Role = "Staff" }
            };

            // Setting up the mock repository to return the userList when GetUsers is called
            _userRepositoryMock.Setup(repo => repo.GetUsers()).Returns(userList);

            // Act
            // Calling the GetUsers method on the controller
            var result = _controller.GetUsers();

            // Assert
            // Verifying the result
            var okResult = result.Result as OkObjectResult; // Checking if the result is an OkObjectResult
            okResult.Should().NotBeNull(); // Asserting that it is not null
            okResult.StatusCode.Should().Be(200); // Asserting that the status code is 200

            var returnedUsers = okResult.Value as IEnumerable<User>; // Extracting the returned users
            returnedUsers.Should().NotBeNull(); // Asserting that the returned users are not null
            returnedUsers.Should().HaveCount(2); // Asserting that the count of returned users is 2
            returnedUsers.Should().BeEquivalentTo(userList); // Asserting that the returned users are equivalent to the expected userList
        }
        [Fact]
        public void GetUserList_ReturnsPaginatedResult_WithCorrectUsers()
        {
            // Arrange
            var userList = new List<User>
            {
                new User { Id = 1, Username = "user1", FirstName = "User", LastName = "One", Phone = "123456", Role = "Admin" },
                new User { Id = 2, Username = "user2", FirstName = "User", LastName = "Two", Phone = "123457", Role = "Staff" },
                new User { Id = 3, Username = "user3", FirstName = "User", LastName = "Three", Phone = "123458", Role = "Staff" },
                new User { Id = 4, Username = "user4", FirstName = "User", LastName = "Four", Phone = "123459", Role = "Staff" }
            };

            // Setup the mock repository to return the userList with pagination applied
            _userRepositoryMock
                .Setup(repo => repo.GetUsersPaginated(1, 2))
                .Returns(userList.Take(2).ToList());

            // Act
            var result = _controller.GetUserList(1, 2);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var returnedUsers = okResult.Value as IEnumerable<UserDetails>;
            returnedUsers.Should().NotBeNull();
            returnedUsers.Should().HaveCount(2);
            returnedUsers.Select(u => u.Username).Should().Contain(new[] { "user1", "user2" });
        }
        
        [Fact]
        public void GetUserList_WithInvalidPaginationParameters_ReturnsEmptyList()
        {
            // Arrange
            var userList = new List<User>(); // Empty list as the expected response

            // Setup the mock repository to return an empty list when invalid parameters are provided
            _userRepositoryMock
                .Setup(repo => repo.GetUsersPaginated(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(userList);

            // Act
            var result = _controller.GetUserList(0, 0); // Invalid pagination parameters

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var returnedUsers = okResult.Value as IEnumerable<UserDetails>;
            returnedUsers.Should().NotBeNull();
            returnedUsers.Should().BeEmpty(); // Ensure that the result is an empty list
        }
        
        [Fact]
        public void GetUserList_WithNoUsersInDatabase_ReturnsEmptyList()
        {
            // Arrange
            var userList = new List<User>(); // Empty list, as there are no users in the database

            // Setup the mock repository to return an empty list when GetUsersPaginated is called
            _userRepositoryMock
                .Setup(repo => repo.GetUsersPaginated(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(userList);

            // Act
            var result = _controller.GetUserList(1, 2); // Valid parameters, but no users in database

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var returnedUsers = okResult.Value as IEnumerable<UserDetails>;
            returnedUsers.Should().NotBeNull();
            returnedUsers.Should().BeEmpty(); // Ensure that the result is an empty list
        }
        
        [Fact]
        public void GetUserList_WithOutOfRangePageNumber_ReturnsEmptyList()
        {
            // Arrange
            var userList = new List<User>
            {
                new User { Id = 1, Username = "user1", FirstName = "User", LastName = "One", Phone = "123456", Role = "Admin" },
                new User { Id = 2, Username = "user2", FirstName = "User", LastName = "Two", Phone = "123457", Role = "Staff" }
            };

            // Setup the mock repository to return an empty list when an out-of-range page is requested
            _userRepositoryMock
                .Setup(repo => repo.GetUsersPaginated(999, 2)) // Out-of-range page number
                .Returns(new List<User>());

            // Act
            var result = _controller.GetUserList(999, 2); // Requesting page 999, which doesn't exist

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var returnedUsers = okResult.Value as IEnumerable<UserDetails>;
            returnedUsers.Should().NotBeNull();
            returnedUsers.Should().BeEmpty(); // Ensure that the result is an empty list
        }
        
        [Fact]
        public void GetUserByUsername_ValidUsername_ReturnsUser()
        {
            // Arrange
            var username = "user1";
            var user = new User 
            { 
                Id = 1, 
                Username = username, 
                FirstName = "User", 
                LastName = "One", 
                Phone = "123456", 
                Role = "Admin" 
            };

            _userRepositoryMock
                .Setup(repo => repo.GetUserByUsername(username.ToLower()))
                .Returns(user);

            // Act
            var result = _controller.GetUserByUsername(username);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var returnedUser = okResult.Value as User;
            returnedUser.Should().NotBeNull();
            returnedUser.Username.Should().Be(username);
            returnedUser.Should().BeEquivalentTo(user);
        }
        
        [Fact]
        public void GetUserByUsername_InvalidUsername_ReturnsNotFound()
        {
            // Arrange
            var username = "nonexistentuser";

            // Setup the mock repository to return null for an invalid username
            _userRepositoryMock
                .Setup(repo => repo.GetUserByUsername(username.ToLower()))
                .Returns((User)null);

            // Act
            var result = _controller.GetUserByUsername(username);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
            notFoundResult.Value.Should().Be("User not found.");
        }
        
        [Fact]
        public void GetUserByUsername_NullOrEmptyUsername_ReturnsBadRequest()
        {
            // Arrange
            string username = null;

            // Act
            var result = _controller.GetUserByUsername(username);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("Invalid username.");

            // Repeat with empty string
            result = _controller.GetUserByUsername("");
            badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("Invalid username.");
        }
        [Fact]
        public void Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "validuser",
                Password = "invalidpassword"
            };

            var user = new User
            {
                Id = 1,
                Username = loginRequest.Username,
                Password = BCrypt.Net.BCrypt.HashPassword("differentpassword"), // Storing a different hashed password
                Role = "Admin"
            };

            // Setup repository mock to return user but invalid password verification
            _userRepositoryMock.Setup(repo => repo.GetUserByUsername(loginRequest.Username.ToLower()))
                .Returns(user);
            _userRepositoryMock.Setup(repo => repo.VerifyPassword(loginRequest.Password, user.Password))
                .Returns(false); // Invalid password

            // Act
            var result = _controller.Login(loginRequest);

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            unauthorizedResult.Should().NotBeNull();
            unauthorizedResult.StatusCode.Should().Be(401);
            unauthorizedResult.Value.Should().Be("Invalid credentials");
        }
        
        [Fact]
        public void Login_NullOrEmptyUsernameOrPassword_ReturnsBadRequest()
        {
            // Arrange
            var loginRequestWithEmptyUsername = new LoginRequest
            {
                Username = "",
                Password = "validpassword"
            };

            var loginRequestWithEmptyPassword = new LoginRequest
            {
                Username = "validuser",
                Password = ""
            };

            // Act & Assert for empty username
            var result = _controller.Login(loginRequestWithEmptyUsername);
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("Invalid username or password");

            // Act & Assert for empty password
            result = _controller.Login(loginRequestWithEmptyPassword);
            badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("Invalid username or password");
        }
        
        //Test for Existing Username on Registration
        [Fact]
        public void Register_ExistingUsername_ReturnsConflict()
        {
            // Arrange
            var existingUser = new User
            {
                Id = 1,
                Username = "existinguser",
                Password = "hashedpassword",
                FirstName = "First",
                LastName = "Last",
                Phone = "123456",
                Role = "Staff"
            };

            var userRegister = new UserRegister
            {
                Username = "existinguser",
                Password = "newpassword",
                FirstName = "New",
                LastName = "User",
                Phone = "789012",
                Role = "Staff"
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByUsername("existinguser")).Returns(existingUser);

            // Act
            var result = _controller.Register(userRegister);

            // Assert
            var conflictResult = result as ConflictObjectResult;
            conflictResult.Should().NotBeNull();
            conflictResult.StatusCode.Should().Be(409);
            conflictResult.Value.Should().Be("Username already exists");
        }
        
        //Test for Deleting a Non-Existent User
        [Fact]
        public void DeleteUserById_NonExistentUser_ReturnsNotFound()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.GetUserById(It.IsAny<int>())).Returns((User)null);

            // Act
            var result = _controller.DeleteUserById(999);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
            notFoundResult.Value.Should().Be("User with ID 999 not found.");
        }
        
       //Test for Registering User with Missing Fields 
        [Fact]
        public void Register_MissingFields_ReturnsBadRequest()
        {
            // Arrange
            var userRegister = new UserRegister
            {
                Username = "",
                Password = "newpassword",
                FirstName = "",
                LastName = "",
                Phone = "789012",
                Role = "Staff"
            };

            // Act
            var result = _controller.Register(userRegister);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("Invalid user registration data. All fields are required.");
        }
        
        [Fact]
        public void UpdateUser_ValidUser_ReturnsOk()
        {
            // Arrange
            var updateUserDto = new UpdateUserDto
            {
                Id = 1,
                FirstName = "UpdatedFirstName",
                LastName = "UpdatedLastName",
                Phone = "1234567890",
                Role = "Admin"
            };

            var existingUser = new User
            {
                Id = 1,
                FirstName = "FirstName",
                LastName = "LastName",
                Phone = "0987654321",
                Role = "Staff"
            };

            _userRepositoryMock.Setup(repo => repo.GetUserById(updateUserDto.Id))
                .Returns(existingUser);

            _userRepositoryMock.Setup(repo => repo.UpdateUserDetails(It.IsAny<User>()));

            // Act
            var result = _controller.UpdateUser(updateUserDto);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().Be("User updated successfully.");

            _userRepositoryMock.Verify(repo => repo.UpdateUserDetails(It.Is<User>(u => 
                u.FirstName == updateUserDto.FirstName &&
                u.LastName == updateUserDto.LastName &&
                u.Phone == updateUserDto.Phone &&
                u.Role == updateUserDto.Role)), Times.Once);
        }
        
        //Test for Invalid User Data
        [Fact]
        public void UpdateUser_InvalidUserData_ReturnsBadRequest()
        {
            // Arrange
            UpdateUserDto invalidDto = null; // null DTO
            var invalidDtoWithNegativeId = new UpdateUserDto
            {
                Id = -1,
                FirstName = "Invalid",
                LastName = "Data",
                Phone = "0000000000",
                Role = "Staff"
            };

            // Act & Assert for null DTO
            var result = _controller.UpdateUser(invalidDto);
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("Invalid user data.");

            // Act & Assert for DTO with negative Id
            result = _controller.UpdateUser(invalidDtoWithNegativeId);
            badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("Invalid user data.");
        }
        
        //Test for Non-Existent User
        [Fact]
        public void UpdateUser_NonExistentUser_ReturnsNotFound()
        {
            // Arrange
            var updateUserDto = new UpdateUserDto
            {
                Id = 999, // Assuming this ID does not exist in the repository
                FirstName = "NonExistent",
                LastName = "User",
                Phone = "1234567890",
                Role = "Admin"
            };

            _userRepositoryMock.Setup(repo => repo.GetUserById(updateUserDto.Id))
                .Returns((User)null); // Mock repository returns null for this ID

            // Act
            var result = _controller.UpdateUser(updateUserDto);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
            notFoundResult.Value.Should().Be($"User with ID {updateUserDto.Id} not found.");
        }
        
        //Test for Server Error
        [Fact]
        public void UpdateUser_ServerError_ReturnsInternalServerError()
        {
            // Arrange
            var updateUserDto = new UpdateUserDto
            {
                Id = 1,
                FirstName = "Test",
                LastName = "User",
                Phone = "1234567890",
                Role = "Admin"
            };

            var existingUser = new User
            {
                Id = 1,
                FirstName = "Existing",
                LastName = "User",
                Phone = "0987654321",
                Role = "Staff"
            };

            _userRepositoryMock.Setup(repo => repo.GetUserById(updateUserDto.Id))
                .Returns(existingUser);

            // Simulate an exception during the update process
            _userRepositoryMock.Setup(repo => repo.UpdateUserDetails(It.IsAny<User>()))
                .Throws(new Exception("Database update error"));

            // Act
            var result = _controller.UpdateUser(updateUserDto);

            // Assert
            var internalServerErrorResult = result as ObjectResult;
            internalServerErrorResult.Should().NotBeNull();
            internalServerErrorResult.StatusCode.Should().Be(500);
            internalServerErrorResult.Value.Should().Be("An error occurred while processing your request.");
        }
        
        
        [Fact]
        public void Login_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "validuser",
                Password = "validpassword"
            };

            var user = new User
            {
                Id = 1,
                Username = loginRequest.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(loginRequest.Password), // Storing hashed password
                Role = "Admin"
            };

            // Setup repository and token service mocks
            _userRepositoryMock.Setup(repo => repo.GetUserByUsername(loginRequest.Username.ToLower()))
                .Returns(user);
            _userRepositoryMock.Setup(repo => repo.VerifyPassword(loginRequest.Password, user.Password))
                .Returns(true);
            _tokenServiceMock.Setup(service => service.GenerateNewToken(user))
                .Returns("sampleToken");
            _tokenServiceMock.Setup(service => service.GenerateRefreshToken())
                .Returns("sampleRefreshToken");

            // Act
            var result = _controller.Login(loginRequest);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var returnedValue = okResult.Value as dynamic;
            returnedValue.Token.Should().Be("sampleToken");
            returnedValue.RefreshToken.Should().Be("sampleRefreshToken");
        }
        
    }
}
