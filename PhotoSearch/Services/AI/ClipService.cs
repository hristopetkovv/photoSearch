namespace PhotoSearch.Services.AI
{
	public class ClipService : IClipService, IDisposable
	{
		private readonly InferenceSession _imageSession;
		private readonly InferenceSession _textSession;
		private readonly ClipTokenizer _tokenizer;

		private static readonly float[] Mean = [0.48145466f, 0.4578275f, 0.40821073f];
		private static readonly float[] Std = [0.26862954f, 0.26130258f, 0.27577711f];

		public ClipService(IWebHostEnvironment env)
		{
			var modelsPath = Path.Combine(env.ContentRootPath, "Models", "clip");

			var options = new Microsoft.ML.OnnxRuntime.SessionOptions();
			options.EnableMemoryPattern = true;
			options.ExecutionMode = ExecutionMode.ORT_SEQUENTIAL;

			_imageSession = new InferenceSession(
				Path.Combine(modelsPath, "vision_model.onnx"), options);
			_textSession = new InferenceSession(
				Path.Combine(modelsPath, "text_model.onnx"), options);

			// Инициализирай tokenizer-а с vocab и merges файловете
			_tokenizer = new ClipTokenizer(
				Path.Combine(modelsPath, "clip_vocab.json"),
				Path.Combine(modelsPath, "clip_merges.txt"));
		}

		public float[] GetImageEmbedding(string imagePath)
		{
			using var image = Image.Load<Rgb24>(imagePath);

			// Стъпка 1: Resize до 224 като запазим aspect ratio
			// (най-малката страна да стане 224)
			var (w, h) = (image.Width, image.Height);
			var scale = 224f / Math.Min(w, h);
			var newW = (int)Math.Round(w * scale);
			var newH = (int)Math.Round(h * scale);
			image.Mutate(x => x.Resize(newW, newH));

			// Стъпка 2: Center crop 224x224
			var cropX = (newW - 224) / 2;
			var cropY = (newH - 224) / 2;
			image.Mutate(x => x.Crop(new Rectangle(cropX, cropY, 224, 224)));

			// Стъпка 3: Попълни тензора с нормализирани стойности
			var tensor = new DenseTensor<float>([1, 3, 224, 224]);

			for (int y = 0; y < 224; y++)
				for (int x = 0; x < 224; x++)
				{
					var px = image[x, y];
					tensor[0, 0, y, x] = (px.R / 255f - Mean[0]) / Std[0];
					tensor[0, 1, y, x] = (px.G / 255f - Mean[1]) / Std[1];
					tensor[0, 2, y, x] = (px.B / 255f - Mean[2]) / Std[2];
				}

			var inputs = new[]
			{
				NamedOnnxValue.CreateFromTensor("pixel_values", tensor)
			};

			using var results = _imageSession.Run(inputs);
			var raw = results.First(r => r.Name == "image_embeds")
							 .AsEnumerable<float>()
							 .ToArray();

			return L2Normalize(raw);
		}

		public float[] GetTextEmbedding(string text)
		{
			var tokens = _tokenizer.Tokenize(text);
			var tensor = new DenseTensor<long>([1, 77]);

			for (int i = 0; i < 77; i++)
				tensor[0, i] = tokens[i];

			var inputs = new[]
			{
				NamedOnnxValue.CreateFromTensor("input_ids", tensor)
			};

			using var results = _textSession.Run(inputs);
			var raw = results.First(r => r.Name == "text_embeds")
							 .AsEnumerable<float>()
							 .ToArray();

			return L2Normalize(raw);
		}

		private static float[] L2Normalize(float[] v)
		{
			var norm = MathF.Sqrt(v.Sum(x => x * x));

			return norm > 0
				? v.Select(x => x / norm).ToArray()
				: v;
		}

		public void Dispose()
		{
			_imageSession.Dispose();
			_textSession.Dispose();
		}
	}
}
