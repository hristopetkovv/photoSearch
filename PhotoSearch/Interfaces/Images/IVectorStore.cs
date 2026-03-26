namespace PhotoSearch.Interfaces.Images
{
	public interface IVectorStore
	{
		void Add(ImageEntry entry);
		void AddRange(List<ImageEntry> entries);
		HashSet<string> GetAllImagePaths();
		IndexingStatusResult IndexingStatus();
		List<ImageSearchResult> Search(float[] textEmbedding, int limit = 20);
	}
}
