namespace PhotoSearch.Tests
{
	public class InMemoryVectorStoreTests
	{
		private readonly InMemoryVectorStore store = new();

		private static ImageEntry CreateEntry(string name, float[] embedding, string path = "") => new()
		{
			ImageName = name,
			ImagePath = path,
			Embedding = new Vector(embedding)
		};

		[Fact]
		public void Add_ShouldAddEntry()
		{
			// Arrange
			var entry = CreateEntry("dog.jpg", [0.1f, 0.2f, 0.3f]);

			// Act
			store.Add(entry);

			// Assert
			store.GetAll().Should().HaveCount(1);
			store.GetAll()[0].Should().BeSameAs(entry);
		}

		[Fact]
		public void AddRange_ShouldAddMultipleEntrie()
		{
			// Arrange
			var entries = new List<ImageEntry>
			{
				CreateEntry("dog.jpg",   [0.1f, 0.2f, 0.3f]),
				CreateEntry("cat.jpg",   [0.4f, 0.5f, 0.6f])
			};

			// Act
			store.AddRange(entries);

			// Assert
			store.GetAll().Should().HaveCount(2);
			store.GetAll().Should().BeEquivalentTo(entries, entries => entries.WithStrictOrdering());
		}

		[Fact]
		public void GetAll_ShouldReturnReadOnlyCollection()
		{
			// Arrange
			var entry = CreateEntry("dog.jpg", [0.1f, 0.2f, 0.3f]);
			store.Add(entry);

			// Act
			var all = store.GetAll();

			// Assert
			all.Should().NotBeNull();
			all.Should().HaveCount(1);
			all[0].Should().BeSameAs(entry);

			var asList = (IList<ImageEntry>)all;
			asList.IsReadOnly.Should().BeTrue();
		}

		[Fact]
		public void GetAllImagePaths_ShouldReturnUniquePaths()
		{
			// Arrange
			var pathA = "/images/dog.jpg";
			var pathB = "/images/cat.jpg";

			store.Add(CreateEntry("dog.jpg", [0.1f, 0.2f, 0.3f], pathA));
			store.Add(CreateEntry("cat.jpg", [0.4f, 0.5f, 0.6f], pathB));
			store.Add(CreateEntry("dog2.jpg", [0.7f, 0.8f, 0.9f], pathA)); // Duplicate path

			// Act
			var paths = store.GetAllImagePaths();

			// Assert
			paths.Should().HaveCount(2);
			paths.Should().Contain(pathA);
			paths.Should().Contain(pathB);
		}

		[Fact]
		public void IndexingStatus_ShouldReportIndexedAndReady_EmptyStore()
		{
			// Act
			var status = store.IndexingStatus();

			// Assert
			status.Indexed.Should().Be(0);
			status.Ready.Should().BeFalse();
		}

		[Fact]
		public void IndexingStatus_ShouldReportIndexedAndReady_WhenEntriesExist()
		{
			// Arrange
			store.Add(CreateEntry("dog.jpg", [0.1f, 0.2f, 0.3f]));
			store.Add(CreateEntry("cat.jpg", [0.4f, 0.5f, 0.6f]));

			// Act
			var status = store.IndexingStatus();

			// Assert
			status.Indexed.Should().Be(2);
			status.Ready.Should().BeTrue();
		}

		[Fact]
		public void Search_ShouldReturnResultsOrderedByScore_AndRespectLimit()
		{
			// Arrange
			var best = CreateEntry("best.jpg", [1f, 0f, 0f]);
			var mid = CreateEntry("mid.jpg", [0.5f, 0f, 0f]);
			var other = CreateEntry("other.jpg", [0f, 1f, 0f]);

			store.AddRange(new List<ImageEntry> { best, mid, other });

			// Act
			var results = store.Search([1f, 0f, 0f], 2);

			// Assert
			results[0].Name.Should().Be("best.jpg");
			results[0].Score.Should().BeApproximately(1f, 0.0001f);

			results[1].Name.Should().Be("mid.jpg");
			results[1].Score.Should().BeApproximately(0.5f, 0.0001f);
		}

		[Fact]
		public void Search_UrlShouldBeEscaped()
		{
			// Arrange
			var name = "a b#.jpg";
			store.Add(CreateEntry(name, [1f, 0f, 0f]));

			// Act
			var results = store.Search([1f, 0f, 0f], 1);

			// Assert
			results.Should().HaveCount(1);
			results[0].Name.Should().Be(name);
			results[0].Url.Should().Be($"/images/{Uri.EscapeDataString(name)}");
		}
	}
}
