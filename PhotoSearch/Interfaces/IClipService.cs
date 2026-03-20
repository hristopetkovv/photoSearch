namespace PhotoSearch.Interfaces
{
	public interface IClipService
	{
		float[] GetImageEmbedding(string imagePath);
		float[] GetTextEmbedding(string text);
	}
}
