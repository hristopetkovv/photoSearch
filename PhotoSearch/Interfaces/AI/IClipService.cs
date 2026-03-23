namespace PhotoSearch.Interfaces.AI
{
	public interface IClipService
	{
		float[] GetImageEmbedding(string imagePath);
		float[] GetTextEmbedding(string text);
	}
}
