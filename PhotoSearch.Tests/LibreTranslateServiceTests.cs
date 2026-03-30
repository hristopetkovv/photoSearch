namespace PhotoSearch.Tests
{
	public class LibreTranslateServiceTests
	{
		[Fact]
		public async Task TranslateToEnglishAsync_ReturnsTranslatedText_WhenApiReturnsSuccess()
		{
			// Arrange
			var expected = "hello";
			var json = JsonSerializer.Serialize(new { translatedText = expected });
			var response = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(json, Encoding.UTF8, "application/json")
			};

			var handler = new FakeHttpMessageHandler(response);
			var httpClient = new HttpClient(handler);

			var config = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["LibreTranslate:Url"] = "http://libre.test"
				})
				.Build();

			var service = new LibreTranslateService(httpClient, config);

			// Act
			var result = await service.TranslateToEnglishAsync("текст");

			// Assert
			result.Should().BeEquivalentTo(expected);
			handler.CapturedRequest.Should().NotBeNull();
			handler.CapturedRequest!.RequestUri.Should().Be("http://libre.test/translate");
		}

		[Fact]
		public async Task TranslateToEnglishAsync_Throws_WhenApiReturnsNonSuccess()
		{
			// Arrange
			var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
			var handler = new FakeHttpMessageHandler(response);
			var httpClient = new HttpClient(handler);

			var config = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["LibreTranslate:Url"] = "http://libre.test"
				})
				.Build();

			var service = new LibreTranslateService(httpClient, config);

			// Act & Assert
			await service.Invoking(s => s.TranslateToEnglishAsync("текст"))
				.Should().ThrowAsync<HttpRequestException>();
		}

		// Simple HttpMessageHandler fake that returns a predetermined response
		private class FakeHttpMessageHandler : HttpMessageHandler
		{
			private readonly HttpResponseMessage response;

			public FakeHttpMessageHandler(HttpResponseMessage response)
			{
				this.response = response;
			}

			public HttpRequestMessage? CapturedRequest { get; private set; }

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				CapturedRequest = request;
				return Task.FromResult(response);
			}
		}
	}
}
