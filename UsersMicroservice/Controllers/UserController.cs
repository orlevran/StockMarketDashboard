using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using UsersMicroservice.Models.DTOs;
using UsersMicroservice.Models;
using UsersMicroservice.Services;

namespace UsersMicroservice.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService service;

        public UserController(IUserService userService)
        {
            service = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegisterRequest user)
        {
            if (user == null)
            {
                return BadRequest("User data is missing");
            }

            var result = await service.RegisterUserAsync(user);
            if (result)
            {
                return Ok("User registered successfully");
            }
            else
            {
                return BadRequest("User already exists or registration failed");
            }
        }

        [HttpGet("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Email and password are required.");
            }

            var user = await service.AuthenticateAsync(request.Email, request.Password);

            if (user == null)
                return Unauthorized();

            return Ok(user);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string? id, [FromQuery] string? email)
        {
            if (string.IsNullOrEmpty(id) && string.IsNullOrEmpty(email))
            {
                return BadRequest("You must provide either an Id or an Email to search.");
            }

            User user;

            if (!string.IsNullOrEmpty(id) && ObjectId.TryParse(id, out ObjectId objectId))
            {
                user = await service.GetUserByIdentifierAsync("_id", id);
            }
            else if (!string.IsNullOrEmpty(email))
            {
                user = await service.GetUserByIdentifierAsync("Email", email);
            }
            else
            {
                return BadRequest("Invalid search criteria.");
            }

            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] Dictionary<string, string>? updates)
        {
            if (updates == null || !updates.Any())
            {
                return BadRequest("No fields provided for update.");
            }

            // Optional: You can validate fields here
            if (updates.ContainsKey("_id"))
            {
                return BadRequest("Cannot update the _id field.");
            }

            var result = await service.UpdateUserFieldsAsync(id, updates);

            if (!result)
            {
                return NotFound("User not found.");
            }

            return Ok("User updated successfully.");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser([FromBody] Dictionary<string, string> body)
        {
            if (body == null || !body.ContainsKey("id") || string.IsNullOrEmpty(body["id"]))
            {
                return BadRequest("User ID is required.");
            }

            var result = await service.DeleteUserAsync(body["id"]);

            if (!result)
            {
                return NotFound("User not found.");
            }

            return Ok("User deleted successfully.");
        }
    }  
}
