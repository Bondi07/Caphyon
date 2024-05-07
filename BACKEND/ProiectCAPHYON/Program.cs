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


builder.Services.AddCors(policyBuilder =>
    policyBuilder.AddDefaultPolicy(policy =>
        policy.WithOrigins("*").AllowAnyHeader().AllowAnyHeader())
);



builder.Services.AddSingleton(provider =>
  {
      var uri = new Uri(neo4jSettings.Uri);
      var authToken = AuthTokens.Basic(neo4jSettings.Username, neo4jSettings.Password);
      var driver = GraphDatabase.Driver(uri, authToken);
      return driver;

  });

builder.Services.AddScoped<IRecipeService, RecipesService>();


var app = builder.Build();

app.UseCors();

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
