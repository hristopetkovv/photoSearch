namespace PhotoSearch.Interfaces
{
	public interface IVectorStore
	{
		void Add(ImageEntry entry);
		List<ImageSearchResult> Search(float[] textEmbedding, int limit = 20);
		IReadOnlyList<ImageEntry> GetAll();
	}
}
