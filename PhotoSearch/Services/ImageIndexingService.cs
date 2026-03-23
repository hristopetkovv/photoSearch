namespace PhotoSearch.Services
{
	public class ImageIndexingService : IHostedService
	{
		private readonly IVectorStore vectorStore;
		private readonly IClipService clipService;
		private readonly IConfiguration configuration;
		private readonly ILogger<ImageIndexingService> logger;
		private readonly IWebHostEnvironment env;

		public ImageIndexingService(
			IVectorStore vectorStore,
			IClipService clipService,
			IConfiguration configuration,
			ILogger<ImageIndexingService> logger,
			IWebHostEnvironment env
			)
		{
			this.vectorStore = vectorStore;
			this.clipService = clipService;
			this.configuration = configuration;
			this.logger = logger;
			this.env = env;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			// Пускаме в background thread за да не блокираме старта на API-то
			_ = Task.Run(() => IndexAllImages(cancellationToken), cancellationToken);
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

		private void IndexAllImages(CancellationToken cancellationToken)
		{
			var folder = Path.GetFullPath(Path.Combine(env.ContentRootPath, configuration["ImageSearch:FolderPath"]!));

			var extensions = configuration
				.GetSection("ImageSearch:SupportedExtensions")
				.Get<string[]>() ?? [".jpg", ".jpeg", ".png"];

			if (!Directory.Exists(folder))
			{
				logger.LogWarning("Папката {Folder} не съществува.", folder);

				return;
			}

			var files = Directory
				.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories)
				.Where(f => extensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
				.ToList();

			logger.LogInformation("Намерени {Count} снимки. Започва индексиране...", files.Count);

			int indexed = 0;

			foreach (var filePath in files)
			{
				if (cancellationToken.IsCancellationRequested) break;

				try
				{
					var embedding = clipService.GetImageEmbedding(filePath);

					vectorStore.Add(new ImageEntry
					{
						ImageName = Path.GetFileName(filePath),
						ImagePath = filePath,
						Embedding = embedding
					});

					logger.LogInformation(
						"Индексирана {File} - embedding[0..4]: {E0} {E1} {E2} {E3} {E4}",
						Path.GetFileName(filePath),
						embedding[0], embedding[1], embedding[2], embedding[3], embedding[4]);

					indexed++;

					if (indexed % 10 == 0)
						logger.LogInformation("Индексирани {Indexed}/{Total}...", indexed, files.Count);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Грешка при индексиране на {File}", filePath);
				}
			}

			logger.LogInformation("Индексирането завърши. {Indexed} снимки готови за търсене.", indexed);

		}
	}
}
