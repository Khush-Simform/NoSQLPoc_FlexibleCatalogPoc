namespace FlexibleCatalogPoc.Data;

public class MongoSettings
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "flexible-catalog";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ConnectionString)
        && !ConnectionString.Contains("USER:PASSWORD", StringComparison.Ordinal)
        && !ConnectionString.Contains("@cluster.mongodb.net", StringComparison.OrdinalIgnoreCase);
}
