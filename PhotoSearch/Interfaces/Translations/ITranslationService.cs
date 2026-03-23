namespace PhotoSearch.Interfaces.Translations
{
	public interface ITranslationService
	{
		Task<string> TranslateToEnglishAsync(string text);
	}
}
