namespace PhotoSearch.Tests
{
	public class PgVectorStoreTests : IAsyncLifetime
	{
		private readonly PostgreSqlContainer container = new PostgreSqlBuilder("pgvector/pgvector:pg16")
			.WithDatabase("photo_search_test")
			.WithUsername("postgres")
			.WithPassword("postgres")
			.Build();

		private AppDbContext context = null!;
		private PgVectorStore store = null!;

		public async Task InitializeAsync()
		{
			await container.StartAsync();

			var options = new DbContextOptionsBuilder<AppDbContext>()
				.UseNpgsql(container.GetConnectionString(), o => o.UseVector())
				.UseSnakeCaseNamingConvention()
				.Options;

			context = new AppDbContext(options);
			await context.Database.EnsureCreatedAsync();

			store = new PgVectorStore(context);
		}

		public async Task DisposeAsync()
		{
			await context.DisposeAsync();
			await container.DisposeAsync();
		}

		private static float[] Embedding(params float[] values)
		{
			var result = new float[512];
			Array.Copy(values, result, Math.Min(values.Length, result.Length));

			return result;
		}

		private static ImageEntry CreateEntry(string name, float[] embedding, string path = "") => new()
		{
			ImageName = name,
			ImagePath = path,
			Embedding = new Vector(embedding)
		};

		[Fact]
		public void Add_ShouldPersistEntry()
		{
			// Arrange
			var entry = CreateEntry("dog.jpg", Embedding(1f, 0f, 0f));

			// Act
			store.Add(entry);

			// Assert
			context.ImageEntries.Count().Should().Be(1);
			store.GetAll().Should().HaveCount(1);
			store.GetAll()[0].ImageName.Should().Be("dog.jpg");
		}

		[Fact]
		public void AddRange_ShouldPersistMultipleEntries()
		{
			// Arrange
			var entries = new List<ImageEntry>
			{
				CreateEntry("dog.jpg",   Embedding(1f, 0f, 0f)),
				CreateEntry("cat.jpg",   Embedding(0f, 1f, 0f)),
				CreateEntry("beach.jpg", Embedding(0f, 0f, 1f))
			};

			// Act
			store.AddRange(entries);

			// Assert
			context.ImageEntries.Count().Should().Be(3);
			store.GetAll().Should().HaveCount(3);
		}

		[Fact]
		public void GetAllImagePaths_ShouldReturnAllPaths()
		{
			// Arrange
			store.Add(CreateEntry("dog.jpg", Embedding(1f, 0f, 0f), "C:/images/dog.jpg"));
			store.Add(CreateEntry("cat.jpg", Embedding(0f, 1f, 0f), "C:/images/cat.jpg"));

			// Act
			var paths = store.GetAllImagePaths();

			// Assert
			paths.Should().HaveCount(2);
			paths.Should().Contain("C:/images/dog.jpg");
			paths.Should().Contain("C:/images/cat.jpg");
		}

		[Fact]
		public void Search_ShouldReturnResultsOrderedByScore()
		{
			// Arrange
			store.AddRange([
				CreateEntry("dog.jpg",   Embedding(1f, 0f, 0f)),
				CreateEntry("cat.jpg",   Embedding(0f, 1f, 0f)),
				CreateEntry("beach.jpg", Embedding(0f, 0f, 1f))
			]);

			// Act
			var results = store.Search(Embedding(1f, 0f, 0f), limit: 3);

			// Assert
			results.Should().HaveCount(3);
			results.First().Name.Should().Be("dog.jpg");
		}

		[Fact]
		public void Search_ShouldRespectLimit()
		{
			store.AddRange([
				CreateEntry("dog.jpg", Embedding(1f, 0f, 0f)),
				CreateEntry("cat.jpg",   Embedding(0f, 1f, 0f)),
				CreateEntry("beach.jpg", Embedding(0f, 0f, 1f))
			]);

			var results = store.Search(Embedding(1f, 0f, 0f), limit: 2);

			results.Should().HaveCount(2);
		}

		[Fact]
		public void IndexingStatus_WhenEmpty_ShouldNotBeReady()
		{
			var status = store.IndexingStatus();

			status.Ready.Should().BeFalse();
			status.Indexed.Should().Be(0);
		}

		[Fact]
		public void IndexingStatus_WhenHasEntries_ShouldBeReady()
		{
			store.Add(CreateEntry("dog.jpg", Embedding(1f, 0f, 0f)));

			var status = store.IndexingStatus();

			status.Ready.Should().BeTrue();
			status.Indexed.Should().Be(1);
		}
	}
}
