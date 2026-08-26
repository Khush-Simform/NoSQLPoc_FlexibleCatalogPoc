using FlexibleCatalogPoc.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FlexibleCatalogPoc.Data;

public class MongoContext
{
    private readonly MongoSettings _settings;
    private readonly Lazy<IMongoDatabase> _database;

    public MongoContext(IOptions<MongoSettings> settings)
    {
        _settings = settings.Value;
        _database = new Lazy<IMongoDatabase>(CreateDatabase);
    }

    public bool IsConfigured => _settings.IsConfigured;

    public IMongoDatabase Database => _database.Value;
    public IMongoCollection<Product> Products => Database.GetCollection<Product>("products");
    public IMongoCollection<Cart> Carts => Database.GetCollection<Cart>("carts");

    private IMongoDatabase CreateDatabase()
    {
        if (!_settings.IsConfigured)
        {
            throw new InvalidOperationException(
                "Set MongoDb:ConnectionString in appsettings.Development.json to your Atlas connection string. See README.md.");
        }

        try
        {
            var client = new MongoClient(_settings.ConnectionString);
            return client.GetDatabase(_settings.DatabaseName);
        }
        catch (Exception ex) when (ex.GetType().Name.Contains("Dns", StringComparison.OrdinalIgnoreCase)
                                   || ex.InnerException is System.Net.Sockets.SocketException)
        {
            throw new InvalidOperationException(
                "MongoDB DNS lookup failed. The host in MongoDb:ConnectionString must come from Atlas → Connect → Drivers " +
                "(it looks like cluster0.xxxxx.mongodb.net). Do not use the placeholder cluster.mongodb.net. " +
                "If the host is already correct, switch Windows DNS to 8.8.8.8 or use Atlas's standard mongodb:// string instead of mongodb+srv://.",
                ex);
        }
    }
}
