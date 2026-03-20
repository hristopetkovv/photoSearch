using Microsoft.AspNetCore.Mvc;
using PhotoSearch.Interfaces;

namespace PhotoSearch.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ImageController : Controller
	{
		private readonly IClipService clipService;
		private readonly IVectorStore vectorStore;

		public ImageController(IClipService clipService, IVectorStore vectorStore)
		{
			this.clipService = clipService;
			this.vectorStore = vectorStore;
		}

		[HttpGet("search")]
		public IActionResult Search([FromQuery] string text, [FromQuery] int limit = 20)
		{
			if (string.IsNullOrWhiteSpace(text))
				return BadRequest("Липсва заявка.");

			var queryEmbedding = clipService.GetTextEmbedding(text);
			var results = vectorStore.Search(queryEmbedding, limit);

			return Ok(results.Select(r => new
			{
				url = $"/images/{Uri.EscapeDataString(r.Entry.ImageName)}",
				fileName = r.Entry.ImageName,
				score = Math.Round(r.Score, 4)
			}));
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
