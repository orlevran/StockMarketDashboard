namespace UsersMicroservice.Models.DTOs
{
    public class UserSearchRequest
    {
        public required string? Id { get; set; }
        public required string? Email { get; set; }
    }
}
