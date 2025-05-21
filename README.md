For security reasons, sensitive configuration values in the appsettings.json file(s) have been omitted.
To run the project locally, you must replace these placeholders with your own settings.

Required Configuration Fields:
1. MongoDBSettings
    * ConnectionString: Your MongoDB cluster connection string
    * DatabaseName: Name of your MongoDB database
    * UsersCollectionName: Name of the users collection
2. Encryption
    * AESKey: Your own 16-character encryption key
    * AESIV: Do not change — this value is preconfigured and must remain unchanged
