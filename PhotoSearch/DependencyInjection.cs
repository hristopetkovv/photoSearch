namespace PhotoSearch
{
	public static class DependencyInjection
	{
		public static void AddServices(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddHostedService<ImageIndexingService>();

			services.AddScoped<ITranslationService, LibreTranslateService>();
			services.AddScoped<IImageService, ImageService>();

			services.AddSingleton<IClipService, ClipService>();

			if (configuration.GetValue<bool>("ImageSearch:UsePostgres"))
				services.AddScoped<IVectorStore, PgVectorStore>();
			else
				services.AddSingleton<IVectorStore, InMemoryVectorStore>();
		}

		public static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddDbContext<AppDbContext>(options =>
			{
				options
					.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), o => o.UseVector())
					.UseSnakeCaseNamingConvention();
			});
		}

		public static void ConfigureStaticFiles(this WebApplication app, IWebHostEnvironment env, IConfiguration configuration)
		{
			app.UseDefaultFiles();
			app.UseStaticFiles(new StaticFileOptions
			{
				FileProvider = new PhysicalFileProvider(Path.GetFullPath(Path.Combine(env.ContentRootPath, configuration["ImageSearch:FolderPath"]!))),
				RequestPath = "/images",
				OnPrepareResponse = context =>
				{
					if (context.File.Name == "index.html")
					{
						context.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store");
						context.Context.Response.Headers.Append("Expires", "-1");
					}
				}
			});
		}
	}
}
