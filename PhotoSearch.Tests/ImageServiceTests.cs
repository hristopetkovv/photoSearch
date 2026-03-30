namespace PhotoSearch.Tests
{
	public class ImageServiceTests : IDisposable
	{
		private readonly string tempRoot;

		public ImageServiceTests()
		{
			tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(tempRoot);
		}

		public void Dispose()
		{
			if (Directory.Exists(tempRoot))
				Directory.Delete(tempRoot, recursive: true);
		}

		[Fact]
		public async Task UploadImageAsync_ShouldReturnFailure_WhenFileIsEmpty()
		{
			// Arrange
			var vectorStore = new FakeVectorStore();
			var clipService = new FakeClipService();
			var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
			{
				["ImageSearch:SupportedExtensions:0"] = ".jpg",
				["ImageSearch:FolderPath"] = "images"
			}).Build();

			var env = new FakeWebHostEnvironment(tempRoot);

			var service = new ImageService(vectorStore, clipService, config, env);

			var file = new TestFormFile(fileName: "empty.jpg", length: 0, content: Array.Empty<byte>());

			// Act
			var result = await service.UploadImageAsync(file);

			// Assert
			result.Success.Should().Be(false);
			result.Message.Should().BeEquivalentTo("Няма файл.");
			vectorStore.AddedEntries.Should().BeEmpty();
		}

		[Fact]
		public async Task UploadImageAsync_ShouldReturnFailure_WhenExtensionNotSupported()
		{
			// Arrange
			var vectorStore = new FakeVectorStore();
			var clipService = new FakeClipService();
			var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
			{
				["ImageSearch:SupportedExtensions:0"] = ".jpg",
				["ImageSearch:FolderPath"] = "images"
			}).Build();

			var env = new FakeWebHostEnvironment(tempRoot);

			var service = new ImageService(vectorStore, clipService, config, env);

			var file = new TestFormFile(fileName: "document.txt", length: 10, content: new byte[10]);

			// Act
			var result = await service.UploadImageAsync(file);

			// Assert
			result.Success.Should().Be(false);
			result.Message.Should().BeEquivalentTo("Неподдържан формат.");
			vectorStore.AddedEntries.Should().BeEmpty();
		}

		[Fact]
		public async Task UploadImageAsync_ShouldSaveFileAndIndex_WhenValidImage()
		{
			// Arrange
			var vectorStore = new FakeVectorStore();
			var capturedPaths = new List<string>();
			var clipService = new FakeClipService(capturedPaths);
			var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
			{
				["ImageSearch:SupportedExtensions:0"] = ".jpg",
				["ImageSearch:FolderPath"] = "images"
			}).Build();

			// ensure images folder exists
			var imagesFolder = Path.Combine(tempRoot, "images");
			Directory.CreateDirectory(imagesFolder);

			var env = new FakeWebHostEnvironment(tempRoot);

			var service = new ImageService(vectorStore, clipService, config, env);

			var content = new byte[] { 1, 2, 3, 4, 5 };
			var file = new TestFormFile(fileName: "photo.jpg", length: content.Length, content: content);

			// Act
			var result = await service.UploadImageAsync(file);

			// Assert
			result.Success.Should().Be(true);
			result.Name.Should().Contain("photo");
			vectorStore.AddedEntries.Should().HaveCount(1);
		}

		// -- Test helpers / fakes --

		private class FakeVectorStore : IVectorStore
		{
			public readonly List<ImageEntry> AddedEntries = new();

			public void Add(ImageEntry entry)
			{
				AddedEntries.Add(entry);
			}

			public void AddRange(List<ImageEntry> entries) => throw new NotImplementedException();

			public IReadOnlyList<ImageEntry> GetAll() => throw new NotImplementedException();

			public HashSet<string> GetAllImagePaths() => throw new NotImplementedException();

			public IndexingStatusResult IndexingStatus() => throw new NotImplementedException();

			public List<ImageSearchResult> Search(float[] textEmbedding, int limit = 20) => throw new NotImplementedException();
		}

		private class FakeClipService : IClipService
		{
			private readonly List<string>? capturedPaths;

			public FakeClipService(List<string>? capturedPaths = null)
			{
				this.capturedPaths = capturedPaths;
			}

			public float[] GetImageEmbedding(string imagePath)
			{
				capturedPaths?.Add(imagePath);

				// return a dummy 512-length embedding (ImageService wraps this into Vector)
				var emb = new float[512];
				emb[0] = 1f;

				return emb;
			}

			public float[] GetTextEmbedding(string text) => throw new NotImplementedException();
		}

		private class FakeWebHostEnvironment : IWebHostEnvironment
		{
			public FakeWebHostEnvironment(string contentRootPath)
			{
				ContentRootPath = contentRootPath;
			}

			public string ApplicationName { get; set; } = string.Empty;
			public IFileProvider? ContentRootFileProvider { get; set; }
			public string ContentRootPath { get; set; }
			public string EnvironmentName { get; set; } = string.Empty;
			public IFileProvider? WebRootFileProvider { get; set; }
			public string WebRootPath { get; set; } = string.Empty;
		}

		private class TestFormFile : IFormFile
		{
			private readonly byte[] data;

			public TestFormFile(string fileName, long length, byte[] content)
			{
				FileName = fileName;
				Length = length;
				data = content ?? Array.Empty<byte>();
				ContentType = "application/octet-stream";
				Name = "file";
			}

			public string ContentType { get; }

			public string ContentDisposition { get; set; } = string.Empty;

			public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

			public long Length { get; }

			public string Name { get; }

			public string FileName { get; }

			public void CopyTo(Stream target)
			{
				target.Write(data, 0, data.Length);
				target.Flush();
			}

			public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
			{
				await target.WriteAsync(data, 0, data.Length, cancellationToken);
				await target.FlushAsync(cancellationToken);
			}

			public Stream OpenReadStream() => new MemoryStream(data);
		}
	}
}
