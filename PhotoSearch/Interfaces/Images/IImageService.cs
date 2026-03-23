namespace PhotoSearch.Interfaces.Images
{
	public interface IImageService
	{
		Task<ImageUploadResult> UploadImageAsync(IFormFile image);
	}
}
