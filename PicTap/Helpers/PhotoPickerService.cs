using System;
using System.IO;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Xamarin.Media;

namespace PicTap
{
	public static class PhotoPickerService
	{
		static MediaFile PhotoMediaFile;

		public static async Task<Stream> ChoosePicture() {

			var picker = new MediaPicker();
			await picker.PickPhotoAsync().ContinueWith(t =>
			{
				PhotoMediaFile = t.Result;
				Console.WriteLine(PhotoMediaFile.Path);
			}, TaskScheduler.FromCurrentSynchronizationContext());

			var photoStream = PhotoMediaFile.GetStream();
			if (photoStream != null) return photoStream;
			else await UserDialogs.Instance.AlertAsync("No photos library accessible", "Photos Library missing", "OK");

			return null;
		}

	}
}

