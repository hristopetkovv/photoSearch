using PhotoSearch.Interfaces;
using PhotoSearch.Models;

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

		public List<(ImageEntry Entry, float Score)> Search(float[] textEmbedding, int limit = 20)
		{
			return entries
					.Select(e => (Entry: e, Score: CosineSimilarity(e.Embedding, textEmbedding)))
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
