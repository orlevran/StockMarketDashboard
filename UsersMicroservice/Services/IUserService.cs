using UsersMicroservice.Models.DTOs;
using UsersMicroservice.Models;

namespace UsersMicroservice.Services
{
    public interface IUserService
    {
        Task<bool> RegisterUserAsync(UserRegisterRequest user);
        Task<User?> AuthenticateAsync(string email, string password);
        Task<User?> GetUserByIdentifierAsync(string field, string identifier);
        Task<bool> UpdateUserFieldsAsync(string id, Dictionary<string, string> updates);
        Task<bool> DeleteUserAsync(string id);
    }
}
