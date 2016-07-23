using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UIKit;
using Foundation;
using CoreImage;
using CoreGraphics;
using Tesseract.iOS;
using System.Text.RegularExpressions;
using Acr.UserDialogs;
using Contacts;
using System.Linq;
using GPUImage.Filters.ColorProcessing;

namespace PicTap
{
	public class ImagePreProcessor
	{
		public IEnumerable<Tesseract.Result> imageResult{ get; set;}
		//IPDFCameraViewController IPDFCamera;

		public ImagePreProcessor ()
		{
		}

		public async Task<IEnumerable<Tesseract.Result>> loadFromPicDivideBy (Tesseract.PageIteratorLevel pageLevel, Stream s){
			TesseractApi api = new TesseractApi ();
			System.Console.WriteLine("INIT TESSERACT -------------------------------------------------------------------------------------");

			try
			{
				if(await api.Init ("eng"))
				if(await api.SetImage (s))
					return api.Results (pageLevel);

			}catch(Exception e){
				System.Console.WriteLine ("[Pics.loadFromPicDivideBy] Error loading picture: "+e.Message);
			}

			return null;
		}

		public async Task loadContactsFromPic(UIImage transformedcropped, bool saveProcessedImage)
		{
			//Preprocess image for better text recognition results
			Stream preProcessedStream = await PreprocessUIImage(
				transformedcropped);
			
			//save for testing purposes
			if (saveProcessedImage) SaveImageToPhotosApp(preProcessedStream, System.DateTime.Now.Second + "bwsharp.png");

			//Tesseract text recognition
			await ReadImageTextThenSaveToPhoneContacts(preProcessedStream);

			Console.WriteLine("loadContactsFromPic Done");
		}

		public async Task ReadImageTextThenSaveToPhoneContacts(Stream bwSharpenedStream)
		{
			Console.WriteLine("In ReadImageTextThenSaveToDB");
			UserDialogs.Instance.ShowLoading("Reading Image...", new MaskType?(MaskType.Clear));

			string error = null;
			string tempError = null;
			string firstname, lastname, number, aff;
			int errorStatus;
			List<CNMutableContact> saveList = new List<CNMutableContact>();
			//CNMutableContact ContactToSave;

			if (bwSharpenedStream == null)
			{
				throw new ArgumentNullException("ReadImageTextThenSaveToDB bwSharpenedStream param is null");
			}

			IEnumerable<Tesseract.Result> imageResult = await 
				loadFromPicDivideBy(Tesseract.PageIteratorLevel.Textline,
					bwSharpenedStream);

			if (imageResult == null)
			{
				UserDialogs.Instance.Alert("Unable to load contacts", "{0} couldn't read the image", "OK");
			}
			else {

				foreach (Tesseract.Result result in imageResult)
				{
					errorStatus = 0;
					firstname = null;
					lastname = null;
					number = null;
					aff = null;

					try
					{
						var nameNumOrgNotesRegex = new Regex(Values.NAMENUMORGNOTESREGEX);
						var nameNumRegex = new Regex(Values.NAMENUMREGEX);
						var wordReg = new Regex(Values.WORDREGEX);
						var numReg = new Regex(Values.NUMREGEX);
						var strictNumReg = new Regex(Values.STRICTNUMREGEX);
						var anyReg = new Regex(Values.ANYSTRINGREGEX);
						var specialReg = new Regex(Values.SINGLESPECIALCHARREGEX);
						var inBetweenNumRegex = new Regex(Values.INVALIDINBETWEENNUMREGEX);

						Console.WriteLine("NEW LINE IN IMAGERESULT: " + result.Text);
						Console.WriteLine("MINUS SPECIAL CHARS: " + RegexHelper.RemoveCountryCodeAndSpecialChar(result.Text, new Regex(Values.COUNTRYCODE), specialReg));

						Match nameNumOrgNotesMatch = nameNumOrgNotesRegex.Match(RegexHelper.RemoveCountryCodeAndSpecialChar(result.Text, new Regex(Values.COUNTRYCODE), specialReg));
						Match nameNumMatch = nameNumRegex.Match(RegexHelper.RemoveCountryCodeAndSpecialChar(result.Text, new Regex(Values.COUNTRYCODE), specialReg));
						Match wordMatch = wordReg.Match(RegexHelper.RemoveCountryCodeAndSpecialChar(result.Text, new Regex(Values.COUNTRYCODE), specialReg));

						if (nameNumOrgNotesMatch.Success /*&& nameNumMatch.Success && wordMatch.Success*/)
						{//check for different types of name and number combinations?
							Console.WriteLine("OVERALL COMBINED MATCH: " + nameNumMatch.Groups[0].Value);
							Console.WriteLine("OVERALL NAME MATCH: " + wordMatch.Groups[0].Value);
							tempError = wordMatch.Groups[0].Value + "\n";

							//processing name
							string[] words = wordMatch.Groups[0].Value.Split(new char[] { ' ', '\t' });
							Console.WriteLine("words split by spaces count: " + words.Length);
							if (words.Length - 1 > 1)
							{
								firstname = words[0];
								for (int i = 1; i < words.Length; i++)
								{
									lastname += words[i];
								}
							}
							else {
								//usually happens in OCR misreads. add to error list for user to manually correct
								lastname = words[0];
								firstname = " ";
								errorStatus = 1;
							}
							Console.WriteLine("firstname: " + firstname + "lastname: " + lastname + " Field missing:" + errorStatus);

							//processing number
							Match numMatch = numReg.Match(RegexHelper.RemoveMatchingString(
								RegexHelper.subtractFromString(wordMatch.Groups[0].Value, nameNumMatch.Groups[0].Value), 
								inBetweenNumRegex));//remove contact name from the name and number portion = number portion

							Console.WriteLine("OVERALL NUMBER MATCH: " + numMatch.Groups[0].Value);

							//remove spaces
							string[] num = numMatch.Groups[0].Value.Split(new char[] { ' ', '\t' });
							Console.WriteLine("number split by spaces count: " + num.Length);
							if (num.Length > 1)
							{
								for (int i = 0; i < num.Length; i++)
								{
									number += num[i];
								}
							}
							else {
								number = numMatch.Groups[0].Value;
							}

							//processing org and notes
							Match orgNotesMatch = anyReg.Match(RegexHelper.subtractFromString(nameNumMatch.Groups[0].Value, nameNumOrgNotesMatch.Groups[0].Value));
							Console.WriteLine("OVERALL ORG AND NOTES MATCH: " + orgNotesMatch.Groups[0].Value);

							string[] orgNotes = orgNotesMatch.Groups[0].Value.Split(new char[] { ' ', '\t' });
							Console.WriteLine("orgNotes split by spaces count: " + orgNotes.Length);
							if (orgNotes.Length > 1)
							{
								for (int i = 0; i < orgNotes.Length; i++)
								{
									aff += orgNotes[i];
								}
							}
							else {
								aff = orgNotesMatch.Groups[0].Value;
							}
							Console.WriteLine("PROCESSING aff --------------------------- aff so far is " + aff);
						}
						else {
							//single word and number - horizontally, vertically
							Console.WriteLine ("PROBLEM IN WORD AND NUMBER MATCHING AFTER READING TEXTLINE FROM IMAGE");
						}

						Console.WriteLine(
							"able to store contact - Name:" + firstname + " ,Last Name:" + lastname + ", Number:" + number);
						var contact = new CNMutableContact { 
							GivenName = firstname, 
							FamilyName = lastname, 
							PhoneNumbers = new CNLabeledValue<CNPhoneNumber>[]{
								new CNLabeledValue<CNPhoneNumber>("mobile", new CNPhoneNumber(number))
							},
							OrganizationName = aff, 
							};
						saveList.Add(contact);
					}
					catch (ArgumentException ae)
					{
						Console.WriteLine(ae.Message + " - duplicatesAllowed?");
					}
					catch (Exception)
					{
						//skip
						Console.WriteLine("ERROR SAVING CONTACT - SKIPPING: " + error);
					}
				}
			}
			UserDialogs.Instance.HideLoading();
			//refractor to save all contacts
			var singlecontacttest = saveList.ElementAt(0);
			/*ContactsHelper.PushNewContactDialogue(singlecontacttest.GivenName, singlecontacttest.FamilyName, 
												  singlecontacttest.PhoneNumbers, singlecontacttest.OrganizationName);
			*/

			await PostImageRecognitionActions.OpenIn(singlecontacttest.GivenName, singlecontacttest.FamilyName,
															  singlecontacttest.PhoneNumbers, singlecontacttest.OrganizationName);


		}

		public string SaveImageToDiskAndPhotosThenReturnPath(Stream s, string filename)
		{
			//save to application folder
			if (s != null && !string.IsNullOrWhiteSpace(filename))
			{
				byte[] imageData = StreamToBytes(s);
				var dir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				var filePath = Path.Combine(dir, filename);

				try
				{
					File.WriteAllBytes(filePath, imageData);

					//notify settingsviewmodel
					Console.WriteLine("writing file done, saving to settingsviewmodel.croppedimagepath");

					//add to user photos app
					SaveImageToPhotosApp(s, filename);
					return filePath;
				}
				catch (System.Exception e)
				{
					System.Console.WriteLine("SaveIamgeToDisk error: {0}", e.Message);
				}
			}
			else {
				throw new ArgumentException("Stream param passed to " +
					"SaveImageToDiskThenNotifyViewModelToStartPreprocessingImage is null");
			}
			Console.WriteLine("SaveImageToDiskThenNotifyViewModelToStartPreprocessingImage done");
			return string.Empty;
		}

		public void SaveImageToPhotosApp(Stream s, string filename){
			var someImage = GetUIImageFromStream(s);
			try{
				someImage.SaveToPhotosAlbum((image, error) => {
					var o = image as UIImage;
					//Console.WriteLine("error:" + error);
				});
				Console.WriteLine("Saved Image to photos app");
			}catch(Exception e){
				Console.WriteLine ("error saving processed image: {0}", e.Message);
			}
		}

		/*public async Task<Stream> PreprocessImage(Stream image, double GaussianSizeX, double GaussianSizeY){
			Console.WriteLine("in PreprocessImage");

			var origImage = GetUIImageFromStream(image);
			Console.WriteLine("converted stream to UIImage");

			var sharpenedImage = UnSharpMask (origImage);
			Console.WriteLine("sharpened image");

			//var croppedImageAsBytes = await Crop (sharpenedImage);
			Console.WriteLine("cropped image");

			return BytesToStream(croppedImageAsBytes);

			//adaptivethreshold here

		}*/



		/*public async Task<Stream> PreprocessImage(string file, double GaussianSizeX, double GaussianSizeY){
			Console.WriteLine("in PreprocessImage:file");
			//UserDialogs.Instance.ShowLoading ("Cleaning Image...");

			var origImage = GetUIImageFromStream(GetStreamFromFilename(file));
			Console.WriteLine("converted stream to UIImage");

			var sharpenedImage = UnSharpMask (origImage);
			Console.WriteLine("sharpened image");

			//UserDialogs.Instance.HideLoading();

			return GetStreamFromUIImage(sharpenedImage);

			//adaptivethreshold here

		}*/

		public async Task<Stream> PreprocessUIImage(UIImage transformedcroppedimage)//, double GaussianSizeX, double GaussianSizeY)
		{
			Console.WriteLine("in PreprocessImage:file");

			if (transformedcroppedimage != null) {
				UserDialogs.Instance.ShowLoading ("Cleaning Image...");

				var sharpenedImage = UnSharpMask(transformedcroppedimage);
				Console.WriteLine("sharpened image");

				var adaptiveThreshImage = AdaptiveThreshold(sharpenedImage);//test

				UIImage finalImage =
					(adaptiveThreshImage == null) ? sharpenedImage : adaptiveThreshImage;

				UserDialogs.Instance.HideLoading();

				return GetStreamFromUIImage(finalImage);
			}
			return null;
		}

		public UIImage AdaptiveThreshold(UIImage inputImage) {
			if (inputImage != null) { 
				var imageFilter = new GPUImageAdaptiveThresholdFilter();
				return imageFilter.CreateFilteredImage(inputImage);
			}

			return null;
		}

		public Task<byte[]> Transform(UIImage image){
			var transformer = new CGAffineTransform ();
			//image.CapInsets.
			//CGAffineTransform.CGRectApplyAffineTransform (CGRect.FromLTRB(0,0,0,0), transformer);
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

		UIImage UnSharpMask(UIImage origImage){

			var imageToSharpen = CIImage.FromCGImage (origImage.CGImage);

			// Create a CIUnsharpMask filter with the input image
			var unsharp_mask = new CIUnsharpMask ()
			{
				Image = imageToSharpen
			};

			// Get the altered image from the filter
			var output = unsharp_mask.OutputImage;

			// To render the results, we need to create a context, and then
			// use one of the context rendering APIs, in this case, we render the
			// result into a CoreGraphics image, which is merely a useful representation
			//
			var context = CIContext.FromOptions (null);

			var cgimage = context.CreateCGImage (output, output.Extent);

			return UIImage.FromImage (cgimage);
		}


		public Stream GetStreamFromFilename(string file){
			Console.WriteLine ("In GetStreamFromFilename");
			return GetStreamFromUIImage (UIImage.FromFile (file));
		}
		public Stream GetStreamFromUIImage(UIImage image){
			Console.WriteLine ("In GetStreamFromUIImage");
			return BytesToStream(UIImageToBytes (image));
		}
		public UIImage GetUIImageFromStream(Stream s){
			Console.WriteLine ("in GetUIImageFromStream");
			return GetImagefromByteArray(StreamToBytes (s));
		}

		public UIImage GetImagefromByteArray (byte[] imageBuffer)
		{
			Console.WriteLine ("in GetImagefromByteArray");
			NSData imageData = NSData.FromArray(imageBuffer);
			Console.WriteLine ("NSData loaded from bytes");
			var img = UIImage.LoadFromData (imageData);
			Console.WriteLine ("UIImage null: {0}", (img == null) ? true : false);
			return img;
		}

		public byte[] StreamToBytes(Stream input)
		{
			Console.WriteLine ("In StreamToBytes");
			using (MemoryStream ms = new MemoryStream()){
				input.CopyTo(ms);
				Console.WriteLine ("bytes copied");
				ms.Seek(0, SeekOrigin.Begin);
				Console.WriteLine ("seekorigin done");
				input.Seek (0, SeekOrigin.Begin);
				Console.WriteLine ("input seek done");
				return ms.ToArray();
			}
		}

		public Stream BytesToStream(byte[] image){
			Console.WriteLine ("In BytesToStream");
			MemoryStream stream = new MemoryStream();
			stream.Write(image, 0, image.Length);
			stream.Seek(0, SeekOrigin.Begin);
			return stream;
		}

		public byte[] UIImageToBytes(UIImage image){
			Byte[] myByteArray = null;
			using (NSData imageData = image.AsPNG()) {
				myByteArray = new Byte[imageData.Length];
				System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes, myByteArray, 0, 
					Convert.ToInt32(imageData.Length));
			}
			return myByteArray;
		}
	}
}

