namespace PhotoSearch.Services.Images
{
	public class InMemoryVectorStore : IVectorStore
	{
		private readonly List<ImageEntry> entries = [];

		public void Add(ImageEntry entry) => entries.Add(entry);

		public IndexingStatusResult IndexingStatus()
		{
			var imagesCount = entries.AsReadOnly().Count;

			return new IndexingStatusResult
			{
				Indexed = imagesCount,
				Ready = imagesCount > 0
			};
		}

		public List<ImageSearchResult> Search(float[] textEmbedding, int limit = 20)
		{
			return entries
					.Select(e => new ImageSearchResult
					{
						Url = $"/images/{Uri.EscapeDataString(e.ImageName)}",
						Name = e.ImageName,
						Score = (float)Math.Round(MathHelper.CosineSimilarity(e.Embedding, textEmbedding), 4)
					})
					.OrderByDescending(x => x.Score)
					.Take(limit)
					.ToList();
		}
	}
}
