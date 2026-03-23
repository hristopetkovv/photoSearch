namespace PhotoSearch.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ImageController : Controller
	{
		private readonly IClipService clipService;
		private readonly IVectorStore vectorStore;
		private readonly ITranslationService translationService;
		private readonly IImageService imageService;

		public ImageController(
			IClipService clipService,
			IVectorStore vectorStore,
			ITranslationService translationService,
			IImageService imageService
			)
		{
			this.clipService = clipService;
			this.vectorStore = vectorStore;
			this.translationService = translationService;
			this.imageService = imageService;
		}

		[HttpGet("search")]
		public async Task<IActionResult> Search([FromQuery] string text, [FromQuery] int limit = 20)
		{
			if (string.IsNullOrWhiteSpace(text))
				return BadRequest("Липсва заявка.");

			var translatedText = await translationService.TranslateToEnglishAsync(text);

			var textEmbedding = clipService.GetTextEmbedding(translatedText);

			return Ok(vectorStore.Search(textEmbedding, limit));
		}

		[HttpGet("status")]
		public IActionResult Status()
			=> Ok(vectorStore.IndexingStatus());

		[HttpPost("upload")]
		public async Task<IActionResult> Upload(IFormFile image)
		{
			var result = await imageService.UploadImageAsync(image);

			return result.Success
				? Ok(result)
				: BadRequest(result.Message);
		}
	}
}
