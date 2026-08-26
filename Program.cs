using FlexibleCatalogPoc.Data;
using FlexibleCatalogPoc.Models;
using FlexibleCatalogPoc.Services;
using MongoDB.Bson.Serialization.Conventions;

ConventionRegistry.Register(
    "flexible-catalog",
    new ConventionPack { new CamelCaseElementNameConvention() },
    _ => true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection(MongoSettings.SectionName));
builder.Services.AddSingleton<MongoContext>();
builder.Services.AddSingleton<CatalogSeeder>();
builder.Services.AddSingleton<ProductService>();
builder.Services.AddSingleton<CartService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new BsonDocumentJsonConverter());
    });
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Flexible Catalog POC",
        Version = "v1",
        Description = "NoSQL product catalog: one collection, category-specific nested attributes, embedded cart + mocked checkout."
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (InvalidOperationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await context.Response.WriteAsJsonAsync(new { message = ex.Message });
    }
});
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();

try
{
    var seeder = app.Services.GetRequiredService<CatalogSeeder>();
    await seeder.SeedAsync();
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "MongoDB seed skipped. Set a real Atlas connection string in appsettings.Development.json.");
}

app.Run();
