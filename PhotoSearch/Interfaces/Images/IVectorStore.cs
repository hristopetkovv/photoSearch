namespace PhotoSearch.Interfaces.Images
{
	public interface IVectorStore
	{
		void Add(ImageEntry entry);
		IndexingStatusResult IndexingStatus();
		List<ImageSearchResult> Search(float[] textEmbedding, int limit = 20);
	}
}
