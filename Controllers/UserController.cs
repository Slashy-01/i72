using I72_Backend.Interfaces;
using I72_Backend.Model;
using I72_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Collections.Generic;

namespace I72_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepository userRepository, ITokenService tokenService, ILogger<UserController> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<IEnumerable<User>> GetUsers()
        {
            try
            {
                var users = _userRepository.GetUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching users");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("List")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<IEnumerable<UserDetails>> GetUserList([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Ensure page and pageSize are valid
                page = Math.Max(1, page);
                pageSize = Math.Max(1, pageSize);
                
                // Fetch paginated users
                var users = _userRepository.GetUsersPaginated(page, pageSize)
                    .Select(user => new UserDetails
                {
                    Id = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Phone = user.Phone,
                    Role = user.Role
                });
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching user list");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{username}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<User> GetUserByUsername(string username)
        {
            try
            {                                                   
                if (string.IsNullOrWhiteSpace(username))    //Added null checks because of failed test cases
                {
                    return BadRequest("Invalid username.");
                }

                var user = _userRepository.GetUserByUsername(username?.Trim().ToLower());
                if (user == null)
                {
                    return NotFound("User not found.");
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while fetching user with username: {username}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                // Check for null or empty username and password
                if (string.IsNullOrWhiteSpace(loginRequest.Username) || string.IsNullOrWhiteSpace(loginRequest.Password))
                {
                    return BadRequest("Invalid username or password");
                }

                var user = _userRepository.GetUserByUsername(loginRequest.Username?.Trim().ToLower());
                if (user == null || !_userRepository.VerifyPassword(loginRequest.Password, user.Password))
                {
                    return Unauthorized("Invalid credentials");
                }

                var tokenString = _tokenService.GenerateNewToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();
                var encodedRefreshToken = _tokenService.EncodeRefreshToken(refreshToken);

                _userRepository.SetUserRefreshToken(user.Username, encodedRefreshToken);

                return Ok(new
                {
                    Token = tokenString,
                    RefreshToken = refreshToken
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult Refresh([FromBody] TokenRefreshRequest request)
        {
            try
            {
                var principal = _tokenService.GetPrincipalFromExpiredToken(request.Token);
                var username = principal.Identity.Name;

                var user = _userRepository.GetUserByUsername(username);
                if (user == null || !_tokenService.ValidateRefreshToken(user.RefreshToken, request.RefreshToken))
                {
                    return Unauthorized("Invalid refresh token");
                }

                var newToken = _tokenService.GenerateNewToken(user);
                var newRefreshToken = _tokenService.GenerateRefreshToken();
                var encodedNewRefreshToken = _tokenService.EncodeRefreshToken(newRefreshToken);

                _userRepository.SetUserRefreshToken(username, encodedNewRefreshToken);

                return Ok(new
                {
                    Token = newToken,
                    RefreshToken = newRefreshToken
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token refresh");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult Register([FromBody] UserRegister userRegister)
        {
            try
            {
                // Check for missing or invalid fields as it caused test case failure
                if (string.IsNullOrWhiteSpace(userRegister.Username) ||
                    string.IsNullOrWhiteSpace(userRegister.Password) ||
                    string.IsNullOrWhiteSpace(userRegister.FirstName) ||
                    string.IsNullOrWhiteSpace(userRegister.LastName) ||
                    string.IsNullOrWhiteSpace(userRegister.Phone))
                {
                    return BadRequest("Invalid user registration data. All fields are required.");
                }
                var username = userRegister.Username?.Trim().ToLower();

                var existingUser = _userRepository.GetUserByUsername(username);
                if (existingUser != null)
                {
                    return Conflict("Username already exists");
                }

                var user = new User
                {
                    Username = username,
                    Password = BCrypt.Net.BCrypt.HashPassword(userRegister.Password),
                    FirstName = userRegister.FirstName,
                    LastName = userRegister.LastName,
                    Phone = userRegister.Phone,
                    Role = string.IsNullOrWhiteSpace(userRegister.Role) ? "Staff" : userRegister.Role
                };

                _userRepository.AddUser(user);

                return Ok(new { Message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user registration");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteUserById(int id)
        {
            try
            {
                var user = _userRepository.GetUserById(id);
                if (user == null)
                {
                    return NotFound($"User with ID {id} not found.");
                }

                _userRepository.DeleteUser(user);
                return Ok($"User with ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting user with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPut("update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateUser([FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                if (updateUserDto == null || updateUserDto.Id <= 0)
                {
                    return BadRequest("Invalid user data.");
                }

                var existingUser = _userRepository.GetUserById(updateUserDto.Id);
                if (existingUser == null)
                {
                    return NotFound($"User with ID {updateUserDto.Id} not found.");
                }

                // Update user details
                existingUser.FirstName = updateUserDto.FirstName;
                existingUser.LastName = updateUserDto.LastName;
                existingUser.Phone = updateUserDto.Phone;
                existingUser.Role = updateUserDto.Role;

                _userRepository.UpdateUserDetails(existingUser);

                return Ok("User updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        
        
    }
}