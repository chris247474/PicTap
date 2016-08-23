using System;
using System.Threading.Tasks;
using CoreGraphics;
using CoreImage;
using GPUImage.Filters.ColorProcessing;
using GPUImage.Filters.ImageProcessing;
using UIKit;

namespace PicTap
{
	public static class OCRPreProcessor
	{
		public static async Task<UIImage> PreprocessUIImage(UIImage transformedcroppedimage,
													 bool saveImage = false, bool useGPU = false)
		{
			Console.WriteLine("in PreprocessImage");

			if (transformedcroppedimage != null)
			{
				//var result = GetStreamFromUIImage(ApplyGreyScale(transformedcroppedimage));
				var scaledImage = ScaleImage(transformedcroppedimage, 960);
				StopWatchHelper.StartTimer();
				var sharpenedImage = UnSharpMask(scaledImage, useGPU);
				StopWatchHelper.StopTimer();
				Console.WriteLine("unsharp timer done");

				/*if (saveImage)
				{
					GlobalVariables.VCToInvokeOnMainThread.InvokeOnMainThread(
						()=>SaveImageToPhotosApp(sharpenedImage, System.DateTime.Now.Second + "bwsharp.png"));
				}*/

				//StopWatchHelper.StartTimer();
				var adaptiveThreshImage = AdaptiveThreshold(sharpenedImage);
				//StopWatchHelper.StopTimer();
				//Console.WriteLine("timer done");

				UIImage finalImage = (adaptiveThreshImage == null) ? sharpenedImage : adaptiveThreshImage;

				if (saveImage)
				{
					GlobalVariables.VCToInvokeOnMainThread.InvokeOnMainThread(
						() => DeviceUtil.SaveImageToPhotosApp(finalImage, 
						                                      DateTime.Now.Second + "bwsharp.png"));
				}

				Console.WriteLine("Done preprocessing");
				return finalImage;
			}
			Console.WriteLine("Failed preprocessing");
			return null;
		}



		public static UIImage ScaleImage(UIImage image, float maxDimension = 960)
		{
			var scaledSize = new CGSize(maxDimension, maxDimension);
			nfloat scaleFactor = 0;

			if (image.Size.Width > image.Size.Height)
			{
				scaleFactor = image.Size.Height / image.Size.Width;
				scaledSize.Width = maxDimension;
				scaledSize.Height = scaledSize.Width * scaleFactor;
			}
			else {
				scaleFactor = image.Size.Width / image.Size.Height;
				scaledSize.Height = maxDimension;
				scaledSize.Width = scaledSize.Height * scaleFactor;
			}

			UIGraphics.BeginImageContext(scaledSize); //UIGraphicsBeginImageContext(scaledSize)
			image.Draw(new CGRect(0, 0, scaledSize.Width, scaledSize.Height));//(CGRectMake(0, 0, scaledSize.width, scaledSize.height))
			var scaledImage = UIGraphics.GetImageFromCurrentImageContext();//scaledImage = UIGraphicsGetImageFromCurrentImageContext()
			UIGraphics.EndImageContext();// UIGraphicsEndImageContext()

			Console.WriteLine("Scaled image to {0}, {1}", scaledImage.Size.Width, scaledImage.Size.Height);

			return scaledImage;
		}

		public static UIImage AdaptiveThreshold(UIImage inputImage)
		{
			if (inputImage != null)
			{
				//var greyScale = new GPUImageGrayscaleFilter();
				var imageFilter = new GPUImageAdaptiveThresholdFilter { BlurRadiusInPixels = 5 };
				return imageFilter.CreateFilteredImage(/*greyScale.CreateFilteredImage(*/inputImage);//));
			}
			return null;
		}

		public static UIImage ApplyGreyScale(UIImage image)
		{
			if (image != null)
			{
				var greyScale = new GPUImageGrayscaleFilter();
				return greyScale.CreateFilteredImage(image);
			}
			return null;
		}

		/*public Task<byte[]> Crop(UIImage image)
		{
			//var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			//var file = Path.Combine (documents, fileName);
		    //var image = UIImage.FromFile(file);

			//try{
				var viewController = new TOCrop.TOCropViewController(image);
				var rootViewController = UIApplication.SharedApplication.KeyWindow.RootViewController;//UIApplication.SharedApplication.Delegate.GetWindow().RootViewController;
				return viewController.ShowCropViewAsync(rootViewController, true, null);
			//}catch(Exception e){
			//	Console.WriteLine ("Crop error: {0}", e.Message);
			//}

			//return null;
		}*/

		static UIImage UnSharpMask(UIImage origImage, bool useGPU = false)
		{
			if (useGPU)
			{
				if (origImage != null)//around the same for Tesseract
				{
					var unsharpFilter = new GPUImageUnsharpMaskFilter()
					{
						BlurRadiusInPixels = (nfloat)6,
						Intensity = (float)0.5
					};
					return unsharpFilter.CreateFilteredImage(origImage);
				}
			}
			else {
				var imageToSharpen = CIImage.FromCGImage(origImage.CGImage);//better for Microsoft Vision

				// Create a CIUnsharpMask filter with the input image
				var unsharp_mask = new CIUnsharpMask()
				{
					Image = imageToSharpen,
					Radius = 7.0f//, Intensity = 1.5f
				};
				// Get the altered image from the filter
				var output = unsharp_mask.OutputImage;

				// To render the results, we need to create a context, and then
				// use one of the context rendering APIs, in this case, we render the
				// result into a CoreGraphics image, which is merely a useful representation
				//
				var context = CIContext.FromOptions(null);

				var cgimage = context.CreateCGImage(output, output.Extent);

				return UIImage.FromImage(cgimage);
			}
			return null;
		}
	}
}

