using PhotoSearch.Interfaces;

namespace PhotoSearch.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ImageController : Controller
	{
		private readonly IClipService clipService;
		private readonly IVectorStore vectorStore;
		private readonly ITranslationService translationService;

		public ImageController(
			IClipService clipService, 
			IVectorStore vectorStore,
			ITranslationService translationService
			)
		{
			this.clipService = clipService;
			this.vectorStore = vectorStore;
			this.translationService = translationService;
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
		{
			var count = vectorStore.GetAll().Count;
			return Ok(new
			{
				indexed = count,
				ready = count > 0
			});
		}
	}
}
