namespace PhotoSearch.Models.Image
{
	public class ImageUploadResult
	{
		public bool Success { get; set; }

		public string? Name { get; set; }

		public string Message { get; set; } = string.Empty;
	}
}
