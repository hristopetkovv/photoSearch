using PhotoSearch.Models;

namespace PhotoSearch.Interfaces
{
	public interface IVectorStore
	{
		void Add(ImageEntry entry);
		List<(ImageEntry Entry, float Score)> Search(float[] textEmbedding, int limit = 20);
		IReadOnlyList<ImageEntry> GetAll();
	}
}
