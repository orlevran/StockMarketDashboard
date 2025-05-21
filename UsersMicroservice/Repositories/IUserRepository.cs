using UsersMicroservice.Models;

namespace UsersMicroservice.Repositories
{
    public interface IUserRepository
    {
        Task<bool> RegisterUserAsync(User user);
        Task<User?> AuthenticateAsync(string email, string password);
        Task<User?> GetUserByIdentifierAsync(string field, string identifier);
        Task<bool> UpdateUserFieldsAsync(string id, Dictionary<string, string> updates);
        Task<bool> DeleteUserAsync(string id);
    }
}
