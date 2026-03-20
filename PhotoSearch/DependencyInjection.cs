using Microsoft.Extensions.FileProviders;
using PhotoSearch.Interfaces;
using PhotoSearch.Services;

namespace PhotoSearch
{
	public static class DependencyInjection
	{
		public static void AddServices(this IServiceCollection services)
		{
			services.AddSingleton<IVectorStore, InMemoryVectorStore>();
			services.AddSingleton<IClipService, ClipService>();
			services.AddHostedService<ImageIndexingService>();
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
