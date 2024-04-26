using Neo4j.Driver;
using ProiectCAPHYON.Configurations;
using ProiectCAPHYON.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var neo4jSettings = builder.Configuration.GetSection("Neo4jSettings").Get<Neo4jSettings>() ?? new Neo4jSettings();
builder.Services.AddSingleton(neo4jSettings);

var username = neo4jSettings.Username;
var password = neo4jSettings.Password;
var driver = GraphDatabase.Driver(neo4jSettings.Uri,
    AuthTokens.Basic(username, password));

await driver.VerifyConnectivityAsync();
Console.WriteLine("Connection Verified!");




var configBuilder = (ConfigBuilder builder) =>
{
    builder
        .WithConnectionTimeout(TimeSpan.FromSeconds(30))
        .WithMaxConnectionLifetime(TimeSpan.FromMinutes(30))
        .WithMaxConnectionPoolSize(10)
        .WithConnectionAcquisitionTimeout(TimeSpan.FromSeconds(20))
        .WithFetchSize(1000);
};

builder.Services.AddSingleton<IDriver>(provider =>
  {
      var uri = new Uri(neo4jSettings.Uri);
      var authToken = AuthTokens.Basic(neo4jSettings.Username, neo4jSettings.Password);
      var driver = GraphDatabase.Driver(uri, authToken);
      return driver;

  });

builder.Services.AddScoped<IRecipeService, RecipesService>();




// Create a Session for the `neo4j` database
using var session = driver.AsyncSession(configBuilder =>
    configBuilder
        .WithDefaultAccessMode(AccessMode.Write)
        .WithDatabase("neo4j"));

//builder.Services.AddSingleton<IDriver>(provider =>
//{
//    var uri = new Uri(neo4jSettings.Uri);
//    var authToken = AuthTokens.Basic(neo4jSettings.Username, neo4jSettings.Password);

//    return GraphDatabase.Driver(uri, authToken);

//});





var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
