using System.Text.Json;
using System.Text.RegularExpressions;

namespace PhotoSearch.Services
{
	public class ClipTokenizer
	{
		private const int MaxLength = 77;
		private const long SotToken = 49406;
		private const long EotToken = 49407;

		private readonly Dictionary<string, long> _vocab;
		private readonly Dictionary<(string, string), int> _bpeRanks;

		public ClipTokenizer(string vocabPath, string mergesPath)
		{
			var json = File.ReadAllText(vocabPath);
			var raw = JsonSerializer.Deserialize<Dictionary<string, long>>(json)!;
			_vocab = raw;
			_bpeRanks = LoadMerges(mergesPath);
		}

		public long[] Tokenize(string text)
		{
			var tokens = new List<long> { SotToken };

			text = CleanText(text);

			foreach (var word in SplitToWords(text))
			{
				var bpeTokens = BPEEncode(word);
				foreach (var token in bpeTokens)
				{
					if (_vocab.TryGetValue(token, out var id))
						tokens.Add(id);

					if (tokens.Count >= MaxLength - 1)
						break;
				}

				if (tokens.Count >= MaxLength - 1)
					break;
			}

			tokens.Add(EotToken);

			while (tokens.Count < MaxLength)
				tokens.Add(0);

			return tokens.Take(MaxLength).ToArray();
		}

		private IEnumerable<string> BPEEncode(string word)
		{
			// Разбий думата на символи и добави </w> към последния
			var symbols = word.Select((c, i) =>
				i == word.Length - 1 ? c + "</w>" : c.ToString()).ToList();

			while (symbols.Count > 1)
			{
				// Намери двойката с най-нисък BPE rank
				var bestPair = ((string, string)?)null;
				var bestRank = int.MaxValue;

				for (int i = 0; i < symbols.Count - 1; i++)
				{
					var pair = (symbols[i], symbols[i + 1]);
					if (_bpeRanks.TryGetValue(pair, out var rank) && rank < bestRank)
					{
						bestRank = rank;
						bestPair = pair;
					}
				}

				if (bestPair is null) break;

				// Слей най-добрата двойка
				var (first, second) = bestPair.Value;
				var merged = new List<string>();

				int j = 0;
				while (j < symbols.Count)
				{
					if (j < symbols.Count - 1 &&
						symbols[j] == first &&
						symbols[j + 1] == second)
					{
						merged.Add(first + second);
						j += 2;
					}
					else
					{
						merged.Add(symbols[j]);
						j++;
					}
				}

				symbols = merged;
			}

			return symbols;
		}

		private static string CleanText(string text)
			=> Regex.Replace(text.ToLowerInvariant().Trim(), @"\s+", " ");

		private static IEnumerable<string> SplitToWords(string text)
			=> Regex.Split(text, @"\s+").Where(w => !string.IsNullOrEmpty(w));

		private static Dictionary<(string, string), int> LoadMerges(string path)
		{
			var merges = new Dictionary<(string, string), int>();
			var lines = File.ReadAllLines(path).Skip(1); // пропусни header реда
			int rank = 0;
			foreach (var line in lines)
			{
				var parts = line.Split(' ');
				if (parts.Length == 2)
					merges[(parts[0], parts[1])] = rank++;
			}
			return merges;
		}
	}
}
