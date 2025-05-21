using MongoDB.Bson;
using UsersMicroservice.Models.DTOs;
using UsersMicroservice.Models;
using MongoDB.Driver;

namespace UsersMicroservice.Services
{
    public class UserService(IMongoDatabase database, EncryptionService encryptionService) : IUserService
    {
        private readonly IMongoCollection<User> users = database.GetCollection<User>("Users");
        private readonly EncryptionService encryptionService = encryptionService;

        public async Task<bool> RegisterUserAsync(UserRegisterRequest user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password) ||
                string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.LastName) ||
                string.IsNullOrEmpty(user.PhoneNumber))
            {
                throw new ArgumentException("Data is missing");
            }

            try
            {
                // Check if the email already exists
                var existingUser = await users.Find(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)).FirstOrDefaultAsync();
                if (existingUser != null)
                {
                    return false; // User already exists
                }

                User result = new User
                {
                    Email = user.Email,
                    Password = encryptionService.Encrypt(user.Password),
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    createdAt = DateTime.UtcNow,
                    lastLogin = DateTime.UtcNow,
                    lastUpdate = DateTime.UtcNow,
                    IsActive = true
                };

                await users.InsertOneAsync(result);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while registering user", ex);
            }
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Email or password is missing");
            }

            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            var user = await users.Find(filter).FirstOrDefaultAsync();

            if (user != null && encryptionService.Decrypt(user.Password).Equals(password))
            {
                var update = Builders<User>.Update.Set(u => u.lastLogin, DateTime.UtcNow);
                var updateResult = await users.UpdateOneAsync(filter, update);
                return user;
            }

            return null;
        }

        public async Task<User?> GetUserByIdentifierAsync(string field, string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            if (string.IsNullOrEmpty(field))
            {
                throw new ArgumentNullException(nameof(field));
            }

            FilterDefinition<User> filter;

            if (field.Equals("_id"))
            {
                filter = Builders<User>.Filter.Eq(field, new ObjectId(identifier));
            }
            else if (field.Equals("Email"))
            {
                filter = Builders<User>.Filter.Eq(field, identifier);
            }
            else
            {
                throw new ArgumentException("User can be found only by _id or email");
            }

            try
            {
                var user = await users.Find(filter).FirstOrDefaultAsync();
                if (user != null && !string.IsNullOrEmpty(user.Password))
                {
                    user.Password = encryptionService.Decrypt(user.Password);
                }

                return user;
            }
            catch (Exception)
            {
                return null;
            }
            
        }

        public async Task<bool> UpdateUserFieldsAsync(string id, Dictionary<string, string> updates)
        {
            var filter = Builders<User>.Filter.Eq("_id", new ObjectId(id));

            // Create a list of update definitions
            var updateDefinitions = new List<UpdateDefinition<User>>();

            foreach (var field in updates)
            {
                if (field.Key.Equals("_id", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Validate the field
                if (!string.IsNullOrEmpty(field.Value))
                {
                    if (field.Key.Equals("Password", StringComparison.OrdinalIgnoreCase))
                    {
                        updateDefinitions.Add(Builders<User>.Update.Set(field.Key, encryptionService.Encrypt(field.Value)));
                    }
                    else
                    {
                        updateDefinitions.Add(Builders<User>.Update.Set(field.Key, field.Value));
                    }
                }
            }

            if (!updateDefinitions.Any())
            {
                return false; // No valid fields to update
            }

            updateDefinitions.Add(Builders<User>.Update.Set("lastUpdate", DateTime.UtcNow));

            var combinedUpdate = Builders<User>.Update.Combine(updateDefinitions);

            var updateResult = await users.UpdateOneAsync(filter, combinedUpdate);

            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (!ObjectId.TryParse(id, out _))
            {
                Console.WriteLine($"[LOG] Invalid ObjectId: {id}");
                return false; // Invalid ObjectId format
            }

            var filter = Builders<User>.Filter.Eq("_id", new ObjectId(id));
            var deleteResult = await users.DeleteOneAsync(filter);

            Console.WriteLine($"[LOG] Delete operation: Acknowledged = {deleteResult.IsAcknowledged}, Deleted Count = {deleteResult.DeletedCount}");

            return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
        }
    }
}
