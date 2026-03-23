var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddServices();

// Configure the HTTP request pipeline.
var app = builder.Build();

if (app.Environment.IsProduction())
{
	app.UseHttpsRedirection();
}

app.ConfigureStaticFiles(builder.Environment, builder.Configuration);

app.MapControllers();

app.Run();
