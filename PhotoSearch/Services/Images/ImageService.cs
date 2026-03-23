namespace PhotoSearch.Services.Images
{
	public class ImageService : IImageService
	{
		private readonly IVectorStore vectorStore;
		private readonly IClipService clipService;
		private readonly IConfiguration configuration;
		private readonly IWebHostEnvironment env;

		public ImageService(
			IVectorStore vectorStore,
			IClipService clipService,
			IConfiguration configuration,
			IWebHostEnvironment env)
		{
			this.vectorStore = vectorStore;
			this.clipService = clipService;
			this.configuration = configuration;
			this.env = env;
		}

		public async Task<ImageUploadResult> UploadImageAsync(IFormFile image)
		{
			if (image.Length == 0)
				return new ImageUploadResult { Success = false, Message = "Няма файл." };

			var imageExtension = Path.GetExtension(image.FileName).ToLowerInvariant();

			var allowedExtensions = configuration.GetSection("ImageSearch:SupportedExtensions").Get<string[]>() ?? [".jpg", ".jpeg", ".png", ".webp", ".bmp"];
			if (!ImageHelper.IsValidExtension(imageExtension, allowedExtensions))
				return new ImageUploadResult { Success = false, Message = "Неподдържан формат." };

			var imageName = ImageHelper.GenerateImageName(image.FileName, imageExtension);
			var imagePath = ImageHelper.BuildImagePath(imageName, env.ContentRootPath, configuration["ImageSearch:ImagesFolder"]!);

			try
			{
				await CopyImageToStreamAsync(image, imagePath);
				IndexImage(imageName, imagePath);

				return new ImageUploadResult { Success = true, Name = imageName, Message = "Снимката е качена и индексирана." };
			}
			catch (Exception ex)
			{
				ImageHelper.DeleteImageIfExists(imagePath);

				return new ImageUploadResult { Success = false, Message = $"Грешка при качване: {ex.Message}" };
			}
		}

		private async Task CopyImageToStreamAsync(IFormFile image, string imagePath)
		{
			await using var stream = File.Create(imagePath);
			await image.CopyToAsync(stream);
		}

		private void IndexImage(string imageName, string imagePath)
		{
			var embedding = clipService.GetImageEmbedding(imagePath);

			vectorStore.Add(new ImageEntry
			{
				ImageName = imageName,
				ImagePath = imagePath,
				Embedding = embedding
			});
		}
	}
}
