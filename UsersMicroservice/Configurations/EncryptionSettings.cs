namespace UsersMicroservice.Configurations
{
    public class EncryptionSettings
    {
        public required string AESKey { get; set; }
        public required string AESIV { get; set; }
    }
}
