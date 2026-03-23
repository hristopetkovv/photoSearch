namespace PhotoSearch.Services
{
	public class InMemoryVectorStore : IVectorStore
	{
		private readonly List<ImageEntry> entries = [];
		public void Add(ImageEntry entry)
		{
			entries.Add(entry);
		}

		public IReadOnlyList<ImageEntry> GetAll()
		{
			return entries.AsReadOnly();
		}

		public List<ImageSearchResult> Search(float[] textEmbedding, int limit = 20)
		{
			return entries
					.Select(e => new ImageSearchResult
					{
						Url = $"/images/{Uri.EscapeDataString(e.ImageName)}",
						Name = e.ImageName,
						Score = (float)Math.Round(CosineSimilarity(e.Embedding, textEmbedding), 4)
					})
					.OrderByDescending(x => x.Score)
					.Take(limit)
					.ToList();
		}

		private static float CosineSimilarity(float[] a, float[] b)
		{
			float dot = 0f;
			for (int i = 0; i < a.Length; i++)
				dot += a[i] * b[i];

			return dot;
		}
	}
}
