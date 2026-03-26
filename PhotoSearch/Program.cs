var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddDbContext(configuration);
builder.Services.AddServices(configuration);

// Configure the HTTP request pipeline.
var app = builder.Build();

if (app.Environment.IsProduction())
{
	app.UseHsts();
	app.UseHttpsRedirection();
}

app.ConfigureStaticFiles(builder.Environment, configuration);

app.MapControllers();

app.Run();
