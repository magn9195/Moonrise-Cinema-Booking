namespace CinemaWebService.BusinessLogic.BusinessHelper
{
	public static class ImageConverter
	{
		public static List<string> ConvertImagesToBase64DataUrls(List<byte[]> images)
		{
			var result = new List<string>();

			if (images == null || !images.Any())
			{
				return new List<string>();
			}

			foreach (var img in images)
			{
				if (img != null && img.Length > 0)
				{
					var base64String = Convert.ToBase64String(img);
					var dataUrl = $"data:image/jpeg;base64,{base64String}";
					result.Add(dataUrl);
				}
			}

			return result;
		}
	}
}
