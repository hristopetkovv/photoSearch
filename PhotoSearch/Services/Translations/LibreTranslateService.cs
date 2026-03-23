namespace PhotoSearch.Services.Translations
{
	public class LibreTranslateService : ITranslationService
	{
		private readonly HttpClient httpClient;
		private readonly string libreTranslateUrl;

		public LibreTranslateService(HttpClient httpClient, IConfiguration configuration)
		{
			this.httpClient = httpClient;
			libreTranslateUrl = configuration["LibreTranslate:Url"]!;
		}

		public async Task<string> TranslateToEnglishAsync(string text)
		{
			var payload = new
			{
				q = text,
				source = "bg",
				target = "en",
				format = "text"
			};

			var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

			var response = await httpClient.PostAsync($"{libreTranslateUrl}/translate", content);
			response.EnsureSuccessStatusCode();

			var json = await response.Content.ReadAsStringAsync();
			var result = JsonSerializer.Deserialize<JsonElement>(json);

			return result.GetProperty("translatedText").GetString() ?? text;
		}
	}
}
