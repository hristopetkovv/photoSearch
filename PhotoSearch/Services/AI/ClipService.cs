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

			var options = new Microsoft.ML.OnnxRuntime.SessionOptions
			{
				EnableMemoryPattern = true,
				ExecutionMode = ExecutionMode.ORT_SEQUENTIAL
			};

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
			using var image = LoadAndPreprocessImage(imagePath);
			var tensor = BuildTensor(image);
			var raw = RunImageModel(tensor);

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

		private static Image<Rgb24> LoadAndPreprocessImage(string imagePath)
		{
			var image = Image.Load<Rgb24>(imagePath);
			ResizeKeepingAspectRatio(image);
			CenterCrop(image);
			return image;
		}

		private static void ResizeKeepingAspectRatio(Image<Rgb24> image)
		{
			var scale = 224f / Math.Min(image.Width, image.Height);
			var newW = (int)Math.Round(image.Width * scale);
			var newH = (int)Math.Round(image.Height * scale);
			image.Mutate(x => x.Resize(newW, newH));
		}

		private static void CenterCrop(Image<Rgb24> image)
		{
			var cropX = (image.Width - 224) / 2;
			var cropY = (image.Height - 224) / 2;
			image.Mutate(x => x.Crop(new Rectangle(cropX, cropY, 224, 224)));
		}

		private static DenseTensor<float> BuildTensor(Image<Rgb24> image)
		{
			var tensor = new DenseTensor<float>([1, 3, 224, 224]);

			for (int y = 0; y < 224; y++)
				for (int x = 0; x < 224; x++)
				{
					var px = image[x, y];
					tensor[0, 0, y, x] = (px.R / 255f - Mean[0]) / Std[0];
					tensor[0, 1, y, x] = (px.G / 255f - Mean[1]) / Std[1];
					tensor[0, 2, y, x] = (px.B / 255f - Mean[2]) / Std[2];
				}

			return tensor;
		}

		private float[] RunImageModel(DenseTensor<float> tensor)
		{
			var inputs = new[] { NamedOnnxValue.CreateFromTensor("pixel_values", tensor) };
			using var results = _imageSession.Run(inputs);
			return results.First(r => r.Name == "image_embeds")
						  .AsEnumerable<float>()
						  .ToArray();
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
