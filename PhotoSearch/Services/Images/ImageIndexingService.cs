namespace PhotoSearch.Services.Images
{
	public class ImageIndexingService : IHostedService
	{
		private readonly IClipService clipService;
		private readonly IConfiguration configuration;
		private readonly ILogger<ImageIndexingService> logger;
		private readonly IWebHostEnvironment env;
		private readonly IServiceScopeFactory scopeFactory;

		public ImageIndexingService(
			IClipService clipService,
			IConfiguration configuration,
			ILogger<ImageIndexingService> logger,
			IWebHostEnvironment env,
			IServiceScopeFactory scopeFactory
			)
		{
			this.clipService = clipService;
			this.configuration = configuration;
			this.logger = logger;
			this.env = env;
			this.scopeFactory = scopeFactory;
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
			using var scope = scopeFactory.CreateScope();
			var vectorStore = scope.ServiceProvider.GetRequiredService<IVectorStore>();

			var newFiles = GetNewFiles(vectorStore);
			if (newFiles.Count == 0)
			{
				logger.LogInformation("Няма нови снимки за индексиране.");
				return;
			}

			logger.LogInformation("{New} нови снимки за индексиране.", newFiles.Count);

			var newEntries = BuildEntries(newFiles, cancellationToken);

			if (newEntries.Count > 0)
				vectorStore.AddRange(newEntries);

			logger.LogInformation("Индексирането завърши. {Indexed} снимки готови за търсене.", newEntries.Count);
		}

		private List<string> GetNewFiles(IVectorStore vectorStore)
		{
			var folder = Path.GetFullPath(
				Path.Combine(env.ContentRootPath, configuration["ImageSearch:FolderPath"]!));

			if (!Directory.Exists(folder))
			{
				logger.LogWarning("Папката {Folder} не съществува.", folder);
				return [];
			}

			var extensions = configuration
				.GetSection("ImageSearch:SupportedExtensions")
				.Get<string[]>() ?? [".jpg", ".jpeg", ".png"];

			var files = Directory
				.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories)
				.Where(f => extensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
				.ToList();

			var existingPaths = vectorStore.GetAllImagePaths();

			return files.Where(f => !existingPaths.Contains(f)).ToList();
		}

		private List<ImageEntry> BuildEntries(List<string> files, CancellationToken cancellationToken)
		{
			var entries = new List<ImageEntry>();
			int indexed = 0;

			foreach (var filePath in files)
			{
				if (cancellationToken.IsCancellationRequested) break;

				try
				{
					var entry = BuildEntry(filePath);
					entries.Add(entry);
					indexed++;

					if (indexed % 10 == 0)
						logger.LogInformation("Индексирани {Indexed}/{Total}...", indexed, files.Count);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Грешка при индексиране на {File}", filePath);
				}
			}

			return entries;
		}

		private ImageEntry BuildEntry(string filePath)
		{
			var embedding = clipService.GetImageEmbedding(filePath);

			logger.LogInformation(
				"Индексирана {File} - embedding[0..4]: {E0} {E1} {E2} {E3} {E4}",
				Path.GetFileName(filePath),
				embedding[0], embedding[1], embedding[2], embedding[3], embedding[4]);

			return new ImageEntry
			{
				ImageName = Path.GetFileName(filePath),
				ImagePath = filePath,
				Embedding = new Vector(embedding)
			};
		}
	}
}
