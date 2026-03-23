namespace PhotoSearch.Services.Helpers
{
	public static class MathHelper
	{
		public static float CosineSimilarity(float[] a, float[] b)
		{
			float dot = 0f;
			for (int i = 0; i < a.Length; i++)
				dot += a[i] * b[i];
			return dot;
		}
	}
}
