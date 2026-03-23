namespace PhotoSearch.Interfaces
{
	public interface ITranslationService
	{
		Task<string> TranslateToEnglishAsync(string text);
	}
}
