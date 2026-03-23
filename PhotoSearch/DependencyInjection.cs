using Microsoft.Extensions.FileProviders;

namespace PhotoSearch
{
	public static class DependencyInjection
	{
		public static void AddServices(this IServiceCollection services)
		{
			services.AddHttpClient();

			services.AddHostedService<ImageIndexingService>();

			services.AddScoped<ITranslationService, LibreTranslateService>();

			services.AddSingleton<IVectorStore, InMemoryVectorStore>();
			services.AddSingleton<IClipService, ClipService>();
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
