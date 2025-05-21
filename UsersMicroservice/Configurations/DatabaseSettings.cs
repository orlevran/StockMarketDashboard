namespace UsersMicroservice.Configurations
{
    public class DatabaseSettings
    {
        public required string ConnectionString { get; set; }
        public required string DatabaseName { get; set; }
        public required string UsersCollectionName { get; set; }
    }
}
