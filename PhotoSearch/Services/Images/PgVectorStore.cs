using Pgvector.EntityFrameworkCore;

namespace PhotoSearch.Services.Images
{
	public class PgVectorStore(AppDbContext context) : IVectorStore
	{
		public void Add(ImageEntry entry)
		{
			context.ImageEntries.Add(entry);
			context.SaveChanges();
		}

		public void AddRange(List<ImageEntry> entries)
		{
			context.ImageEntries.AddRange(entries);
			context.SaveChanges();
		}

		public IReadOnlyList<ImageEntry> GetAll() => context.ImageEntries.ToList().AsReadOnly();

		public HashSet<string> GetAllImagePaths() => context.ImageEntries.Select(e => e.ImagePath).ToHashSet();

		public IndexingStatusResult IndexingStatus()
		{
			var imagesCount = context.ImageEntries.Count();

			return new IndexingStatusResult
			{
				Indexed = imagesCount,
				Ready = imagesCount > 0
			};
		}

		public List<ImageSearchResult> Search(float[] textEmbedding, int limit = 20)
		{
			var embedding = new Vector(textEmbedding);

			return context.ImageEntries
			.OrderBy(e => e.Embedding.CosineDistance(embedding))
			.Take(limit)
			.Select(e => new ImageSearchResult
			{
				Url = $"/images/{Uri.EscapeDataString(e.ImageName)}",
				Name = e.ImageName,
				Score = (float)Math.Round(1 - e.Embedding.CosineDistance(embedding), 4)
			})
			.ToList();
		}
	}
}
