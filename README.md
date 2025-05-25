For security reasons, sensitive configuration values in the appsettings.json files (each microservice has its configuration file) have been omitted.
To run the project locally, you must replace these placeholders with your settings.

Required Configuration Fields:
1. MongoDBSettings
    * ConnectionString: Your MongoDB cluster connection string
    * DatabaseName: Name of your MongoDB database
    * UsersCollectionName: Name of the users collection
2. Encryption
    * AESKey: Your 16-character encryption key
    * AESIV: Do not change — this value is preconfigured and must remain unchanged
3. API keys
    * AlphaVantage.ApiKey: Some keys may cause inaccurate results. Be aware of the permissions granted by the key you provide.
