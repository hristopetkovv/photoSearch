namespace PhotoSearch.Services.Helpers
{
	public static class ImageHelper
	{
		public static bool IsValidExtension(string extension, string[] allowedExtensions)
			=> allowedExtensions.Contains(extension);

		public static string GenerateImageName(string imageName, string extension)
		{
			var originalName = Path.GetFileNameWithoutExtension(imageName);

			return string.IsNullOrWhiteSpace(originalName)
				? $"{Guid.NewGuid()}{extension}"
				: $"{originalName}_{Guid.NewGuid()}{extension}";
		}

		public static string BuildImagePath(string imageName, string contentRootPath, string imagesFolder)
			=> Path.Combine(Path.GetFullPath(Path.Combine(contentRootPath, imagesFolder)), imageName);

		public static void DeleteImageIfExists(string imagePath)
		{
			if (File.Exists(imagePath))
				File.Delete(imagePath);
		}
	}
}
