using MongoDB.Driver;
using UsersMicroservice.Configurations;
using UsersMicroservice.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// ✅ Load Configuration from appsettings.json
builder.Services.Configure<EncryptionSettings>(builder.Configuration.GetSection("Encryption"));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("MongoDBSettings"));

// ✅ Register MongoDB Client
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;

    // More graceful exception handling
    if (string.IsNullOrEmpty(settings.ConnectionString))
    {
        throw new ArgumentException("MongoDB ConnectionString is not configured properly.");
    }
    return new MongoClient(settings.ConnectionString);
});

// ✅ Register IMongoDatabase safely
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;

    if (string.IsNullOrEmpty(settings.DatabaseName))
    {
        throw new ArgumentException("DatabaseName is not configured properly.");
    }
    return client.GetDatabase(settings.DatabaseName);
});

// ✅ Register Services
builder.Services.AddSingleton<EncryptionService>();
builder.Services.AddSingleton<IUserService, UserService>();

// ✅ Add Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ✅ Swagger UI Configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "StockMarket Dashboard API V1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


// ✅ Test Route for Encryption
app.MapGet("/api/test-encryption", (EncryptionService encryptionService) =>
{
    try
    {
        string originalText = "HelloWorld123";
        string encryptedText = encryptionService.Encrypt(originalText);
        string decryptedText = encryptionService.Decrypt(encryptedText);

        return Results.Ok(new
        {
            Original = originalText,
            Encrypted = encryptedText,
            Decrypted = decryptedText
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Encryption Test Failed: {ex.Message}");
    }
});

// ✅ Test POST Route
app.MapPost("/api/test", () =>
{
    Console.WriteLine("Endpoint hit!");
    return Results.Ok("Endpoint hit!");
});

// ✅ Run Application
app.Run();
