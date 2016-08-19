using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UIKit;
using Foundation;
using CoreImage;
using CoreGraphics;
using System.Text.RegularExpressions;
using Acr.UserDialogs;
using Contacts;
using System.Linq;
using GPUImage.Filters.ColorProcessing;
using System.Threading;
using Microsoft.ProjectOxford.Vision.Contract;
using Microsoft.ProjectOxford.Vision;
using Tesseract.iOS;
using GPUImage.Filters.ImageProcessing;
using Tesseract;

namespace PicTap
{
	public class ImagePreProcessor
	{
		const int SUCCESSTHRESHOLD = 28;
		public const double TIMEOUTLIMIT = 8000;
		CNMutableContact contact;
		CNMutablePostalAddress postalAddress;
		bool nameFound = false, numFound = false, emailFound = false, URLFound = false, //addressFound = false,
				orgFound = false, goToNextIteration = false, streetFound = false, citypostalFound = false,
				cityFound = false, jobFound = false, allowCancelMicrosoftVisionOCRTask = false;

		public ImagePreProcessor()
		{
			
		}

		void ResetContactReaderData() {
			nameFound = false;
			numFound = false;
			emailFound = false;
			URLFound = false;
			orgFound = false;
			goToNextIteration = false;
			streetFound = false;
			citypostalFound = false;
			cityFound = false;
			jobFound = false;

			postalAddress = new CNMutablePostalAddress();
			contact = new CNMutableContact();
		}



		public async Task ShowUserCropIfTextRecognitionTakesTooLong(Stream bwSharpenedStream)
		{
			/*SampleTimer = new Timer(5000);
			SampleTimer.Elapsed += (sender, e) =>
			{
				// Update position slider
				Position.BeginInvokeOnMainThread(() =>
				{
					Position.Value = ThisApp.Input.Device.LensPosition;
				});
			};*/

			//var usercrop = await UserCrop(bwSharpenedStream);
			//await ReadImageTextThenSaveToPhoneContacts(usercrop);
			return;
		}



		/*public async Task<MatchFormat> ReadAnyNameReturnMatchAndRemainderString(string input)
		{
			string[] words;
			List<string> names = new List<string>();
			string remaining = string.Empty;
			var WordRegex = new Regex(RegexHelper.PERSONSNAMEREGEX);
			Match wordMatch = WordRegex.Match(
				RegexHelper.RemoveCountryCodeReadingErrorsAndSpecialChar(
					input));

			Console.WriteLine("input is {0}", input);

			if (wordMatch.Success)
			{//for (int n = 0; wordMatch.Success;n++) {
				Console.WriteLine("WORD MATCH: " + wordMatch.Groups[0].Value);
				words = wordMatch.Groups[0].Value.Split(new char[] { ' ', '\t' });
				Console.WriteLine("words split by spaces count: " + words.Length);
				for (int i = 0; i < words.Length; i++)
				{
					names.Add(words[i]);
				}
				try
				{
					remaining = input.Replace(wordMatch.Groups[0].Value, "");
				}
				catch (Exception e)
				{
					Console.WriteLine("ReadAnyNameReturnMatchAndRemainderString error: {0}", e.Message);
				}
				return new MatchFormat(names.ToArray(), remaining);
			}
			else return null;
		}

		public async Task<NameMatchFormat> ReadSingleNameThenReturnMatchFormat(string severalnames)
		{
			var singleNameRegex = new Regex(RegexHelper.SINGLENAMEREGEX);
			Match singleNameMatch = singleNameRegex.Match(severalnames);
			string name = string.Empty, remaining = string.Empty;
			//string[] names;
			//List<string> nameMatches = new List<string>();

			if (singleNameMatch.Success)
			{
				name = singleNameMatch.Groups[0].Value;
				//nameMatches.Add(name);
				remaining = severalnames.Replace(name, string.Empty);
			}

			return new NameMatchFormat(name, remaining);
		}

		public async Task<MatchFormat> ReadAnyPhoneNumReturnMatchAndRemainderString(string input)
		{
			var numReg = new Regex(RegexHelper.MULTILABELEDNUMREGEX);
			input = RegexHelper.RemoveCountryCodeReadingErrorsAndSpecialChar(input);
			Console.WriteLine("filtered input: {0}", input);
			Match numMatch = numReg.Match(input);

			var strictNumRegex = new Regex(RegexHelper.STRICTNUMREGEX);
			string strictMatchString = string.Empty;

			string remaining = string.Empty;
			List<string> matches = new List<string>();

			if (numMatch.Success)
			{
				var number = string.Empty;
				var rawMatchString = numMatch.Groups[0].Value;
				var originalMatchString = rawMatchString;
				Console.WriteLine("Found labeled number match: {0}", rawMatchString);
				var numLabelReg = new Regex(RegexHelper.LABELREGEX);
				Match labelMatch = numLabelReg.Match(rawMatchString);
				if (labelMatch.Success)
				{
					Console.WriteLine("found number label: {0}", labelMatch.Groups[0].Value);
					try
					{
						var temp = rawMatchString.Replace(labelMatch.Groups[0].Value, string.Empty);
						if (!string.IsNullOrWhiteSpace(temp)) rawMatchString = temp;
					}
					catch (Exception e)
					{
						Console.WriteLine("ReadAnyPhoneNumReturnMatchAndRemainderString error: {0}", e.Message);
					}

					Console.WriteLine("Removed number label: {0}", rawMatchString);
				}

				Match strictMatch = strictNumRegex.Match(rawMatchString);
				if (strictMatch.Success)
				{
					strictMatchString = strictMatch.Groups[0].Value;

					Console.WriteLine("EXACT NUMBER MATCH: " + strictMatchString);

					//remove spaces
					string[] num = strictMatchString.Split(new char[] { ' ', '\t' });
					Console.WriteLine("number split by spaces count: " + num.Length);
					if (num.Length > 1)
					{
						for (int i = 0; i < num.Length; i++)
						{
							number += num[i];
						}
					}
					else {
						number = strictMatchString;
					}
					try
					{
						remaining = input.Replace(originalMatchString, "");
					}
					catch (Exception e)
					{
						Console.WriteLine("ReadAnyPhoneNumReturnMatchAndRemainderString error: {0}", e.Message);
					}
					Console.WriteLine("extracted number is {0}", number);
					matches.Add(number);

					return new MatchFormat(matches.ToArray(), remaining);
				}
				else Console.WriteLine("No strict number match found");
			}
			return null;
		}
		public async Task<MatchFormat> ReadAnyEmailReturnMatchAndRemainderString(string input)
		{
			input = //RegexHelper.RemoveCountryCodeReadingErrorsAndSpecialChar(
				RegexHelper.RemoveCommonTesseractEmailErrors(input);//);
			Console.WriteLine("input filtered errors and special chars:{0}", input);

			var emailRegex = new Regex(RegexHelper.LABELEDEMAILREGEX);
			var emailMatch = emailRegex.Match(input);

			//data for MatchFormat
			List<string> emailMatches = new List<string>();//just in case this funciton is reused for mutiple contact lists
			string remaining = string.Empty;

			if (emailMatch.Success)
			{
				var labeledEmailMatchString = emailMatch.Groups[0].Value;
				var labelReg = new Regex(RegexHelper.LABELREGEX);
				Match labelMatch = labelReg.Match(labeledEmailMatchString);

				//remove  label if any
				if (labelMatch.Success)
				{
					Console.WriteLine("found email label: {0}", labelMatch.Groups[0].Value);
					try
					{
						var temp = labeledEmailMatchString.Replace(labelMatch.Groups[0].Value, string.Empty);
						if (!string.IsNullOrWhiteSpace(temp)) labeledEmailMatchString = temp;
					}
					catch (Exception e)
					{
						Console.WriteLine("ReadAnyEmailReturnMatchAndRemainderString error: {0}", e.Message);
					}
					Console.WriteLine("Removed number label: {0}", labeledEmailMatchString);
				}

				//add to MatchFormat
				if (!string.IsNullOrWhiteSpace(labeledEmailMatchString)) emailMatches.Add(labeledEmailMatchString);
				else Console.WriteLine("labeledEmailMatch is nullemptyorwhitespace: {0}", labeledEmailMatchString);
				try
				{
					remaining = input.Replace(labeledEmailMatchString, "");
				}
				catch (Exception e)
				{
					Console.WriteLine("ReadAnyEmailReturnMatchAndRemainderString error: {0}", e.Message);
				}

				Console.WriteLine("Remaining input is {0}", remaining);
				var matchFormat = new MatchFormat(emailMatches.ToArray(), remaining);
				return matchFormat;
			}
			else {
				Console.WriteLine("No email found in this textline");
			};
			return null;
		}

		public async Task<MatchFormat> ReadAnyURLReturnMatchAndRemainderString(string input)
		{
			input = //RegexHelper.RemoveCountryCodeReadingErrorsAndSpecialChar(
				RegexHelper.RemoveCommonTesseractURLErrors(input);//);
			Console.WriteLine("URL: input filtered errors and special chars:{0}", input);

			var urlRegex = new Regex(RegexHelper.WWW_LIMITEDURLREGEX);
			var urlMatch = urlRegex.Match(input);

			//data for MatchFormat
			List<string> urlMatches = new List<string>();//just in case this funciton is reused for mutiple contact lists
			string remaining = string.Empty;

			for (int c = 0; c < input.Length; c++)
			{
				if (urlMatch.Success)
				{
					var urlLabeledMatchString = urlMatch.Groups[0].Value;

					//add to MatchFormat
					if (!string.IsNullOrWhiteSpace(urlLabeledMatchString)) urlMatches.Add(urlLabeledMatchString);
					else Console.WriteLine("labeledEmailMatch is nullemptyorwhitespace: {0}", urlLabeledMatchString);
					try
					{
						remaining = input.Replace(urlLabeledMatchString, "");
					}
					catch (Exception e)
					{
						Console.WriteLine("ReadAnyURLReturnMatchAndRemainderString error: {0}", e.Message);
					}

					Console.WriteLine("Remaining input is {0}", remaining);
					var matchFormat = new MatchFormat(urlMatches.ToArray(), remaining);
					return matchFormat;
				}
				else {
					Console.WriteLine("No url found starting at string position {0} this textline", c);
					urlMatch = urlRegex.Match(input, c);
				};
			}

			return null;
		}

		public void FindTextRegion()
		{
			CITextFeature ci = new CITextFeature();


		}*/

		public async Task<int> TesseractExtractText(Stream s, Tesseract.OcrEngineMode engineMode,
										  Tesseract.PageSegmentationMode segMode, string correctOutput, int ctr = 0)
		{
			Console.WriteLine("Begin Tesseract Test: EngineMode {0}, SegmentationMode: {1}",
							  engineMode,
							  segMode);

			string text = string.Empty;
			int LD = 1000000;

			try
			{
				text = await /*ExtractTextFromImage_Tesseract(s, segMode, engineMode);*/ExtractTextFromImage_ProjectOxford(s);//, segMode, engineMode);//ExtractTextFromImage_ProjectOxford(s);

				Console.WriteLine("Extracted text is {0}\n", text);
				Console.WriteLine("Correct output is {0}\n", correctOutput);
				LD = Levenshtein.LD(text, correctOutput);
				Console.WriteLine("LD is {0}", LD);
			}
			catch (Exception e)
			{
				Console.WriteLine("[TesseractExtractText] Error loading picture at test #{0}: {1}", ctr, e.Message);
			}

			Console.WriteLine("Test #{0} done", ctr);
			return (LD >= SUCCESSTHRESHOLD) ? 0 : 1;
		}

		public async Task<int> ExtractTextTest(Stream s, Tesseract.OcrEngineMode engineMode,
										  Tesseract.PageSegmentationMode segMode, string correctOutput, int ctr)
		{//create auto test functions and data
			//Console.WriteLine("INIT TESSERACT -------------------------------------------------------------------------------------");
			string text = string.Empty;
			int successScore = 0;

			successScore += (await TesseractExtractText(s, engineMode, segMode, correctOutput, ctr));

			return successScore;
		}

		string[] GetCorrectStringOutputs()
		{
			var correctText = new List<string>();//added in order of test#.jpg
												 //test0.jpg
			correctText.Add("Website and Graphic Designer\nDesign Point\nWebsites | Logos | Graphics\nM: 0421 788 830\n" +
							"E: info@designpoint.com.au\nW:www.designpoint.com.au");
			//test1.jpg
			correctText.Add("ELORDE BOXING GYM\nwww.elordeboxinggym.com www.elordeboxing.com\nCucuy Elorde\n(Smart) 0918-9307786" +
							"(Sun) 0922-8307786\ncucuy_elorde@yahoo.com.ph\nelorde_franchise@yahoo.com\nwww.facebook.com/cucuyelorde\n" +
							"www.facebook.com/elordegymandfitness");
			//test2.jpg
			correctText.Add("UNIONBANK\nAaron James B. Urena\nRelationship Manager\nRBC\n355 AGCOR Building\nKatipunan Avenue" +
							", Loyola Heights\nQuezon City\nCellphone: 0917-8124183\najburena@unionbankph.com");
			//test3.jpg
			correctText.Add("Fervor\nMIKE FARAG\nPH 9132846455\nEM mike@createfervor.com\nTW @mikefarag01");
			//test4.jpg
			correctText.Add("AZUL\nWORLD\nPARTNERSHIP FOR PROGRESS\nTel +65 6273 3647 DID: + 65 6273 3648\nM: +65 9827 3474" +
							" E: larry@azulworld.com\nwww.AZULWORLD.com\nLARRY LAM, Chairman\n158 Cecil Street, #11-01, " +
							"Singapore 069545");
			//test5.jpg
			correctText.Add("AGC\nShinichi Otaka\nVice-President\nAccounting/Finance\nAGC FLAT GLASS PHILIPPINES, INC.\n" +
							"Asahi Special Economic Zone (ASEZ)\n730 M.H. Del Pilar St., Bgy. Pinagbuhatan, Pasig City," +
							"Phillippines 1602\nTel: +632 641-1981 to 87 loc. 105\nFax No.: +632 641-1988 www.agc-flatglass.ph\n" +
							"Email: shinichi-otaka@agc.com www.yourglass.com");
			//test6.jpg
			correctText.Add("Victor Emmanuel T. Tiongson\nORIX\nFirst Vice-President\nHead of Treasury\nORIX METRO" +
							" Leasing and Finance Corporation\n21st floor, GT Tower International, Ayala Avenue\n" +
							"cor. H.V. Dela Costa St., 1227 Makati, Philippines\nTel. Nos.: (632) 858-8888, 982-9400 " +
							"loc. 523\nD.L.: 858-8826 Fax No.: 858-8835\nEmail: vttiongson@orix.com.ph Website: www.orix.com.ph");

			//test7.jpg
			correctText.Add("ORIX Liezl S. Romero\nAssistant Vice President\nORIX METRO Leasing and Finance Corporation\n" +
							"21st Floor, GT Tower International, Ayala Avenue\n" +
							"cor. H.V. Dela Costa St., 1227 Makati, Philippines\nTel. Nos.: (632) 858-8888, 982-9400 loc. 234" +
							"\nFax No.: 858-8824 Cell No.: 0918-906-4379\n" +
							"E-mail: Isromeo@orix.com.ph Website: www.orix.com.ph");

			//test8.jpg
			correctText.Add("BPI CAPITAL\nCorporation\nMichael Vicente A. Mate\nSenior Associate\n8th Floor" +
							", BPI Building\nAyala Avenue corner Paseo de Roxas, Makati City 1226\nTel. No.: (632) " +
							"816-9756 Fax No.: (632) 818-7809\nMobile No.: (+63) 917-5787611\nEmail: mvamate@bpi.com.ph\n" +
							"www.bpiexpressonline.ph");

			//test9.jpg
			//correctText.Add("");

			return correctText.ToArray();
		}



		public async Task RunBusinessCardTest(UIImage[] inputImages)
		{
			List<TestData> averageScores = new List<TestData>();

			for (int c = 0; c < inputImages.Length; c++) {
				Console.WriteLine("processing image {0} for test array", c);
				var preProcessedImage = await PreprocessUIImage(inputImages[c], true);
				//var scaledImage = ScaleImage(inputImages[c]);
				inputImages[c] = preProcessedImage;
				Console.WriteLine("Done processing image {0} for test array", c);
			}

			UserDialogs.Instance.ShowLoading();

			//tesseract only - all possible pagesegmentationmodes
			//await TaskHelper.RunTaskThenCancelAfterTimeout(async () => {
			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractOnly,
											Tesseract.PageSegmentationMode.Auto));
			//});

			/*averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractOnly,
														Tesseract.PageSegmentationMode.AutoOsd));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractOnly,
													Tesseract.PageSegmentationMode.CircleWord));


			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractOnly,
													Tesseract.PageSegmentationMode.SingleBlock));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractOnly,
													Tesseract.PageSegmentationMode.SingleBlockVertText));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractOnly,
													Tesseract.PageSegmentationMode.SingleChar));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractOnly,
													Tesseract.PageSegmentationMode.SingleColumn));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractOnly,
													Tesseract.PageSegmentationMode.SingleLine));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractOnly,
													Tesseract.PageSegmentationMode.SingleWord));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractOnly,
													Tesseract.PageSegmentationMode.SparseText));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractOnly,
													Tesseract.PageSegmentationMode.SparseTextOsd));


			//cubonly - all possible segmentationmodes
			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.CubeOnly,
														Tesseract.PageSegmentationMode.Auto));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.CubeOnly,
														Tesseract.PageSegmentationMode.AutoOsd));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.CubeOnly,
													Tesseract.PageSegmentationMode.CircleWord));


			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.CubeOnly,
													Tesseract.PageSegmentationMode.SingleBlock));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.CubeOnly,
													Tesseract.PageSegmentationMode.SingleBlockVertText));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.CubeOnly,
													Tesseract.PageSegmentationMode.SingleChar));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.CubeOnly,
													Tesseract.PageSegmentationMode.SingleColumn));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.CubeOnly,
													Tesseract.PageSegmentationMode.SingleLine));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.CubeOnly,
													Tesseract.PageSegmentationMode.SingleWord));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.CubeOnly,
													Tesseract.PageSegmentationMode.SparseText));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.CubeOnly,
													Tesseract.PageSegmentationMode.SparseTextOsd));


			//combined - all possible segmentationmodes
			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractCubeCombined,
														Tesseract.PageSegmentationMode.Auto));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractCubeCombined,
														Tesseract.PageSegmentationMode.AutoOsd));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractCubeCombined,
													Tesseract.PageSegmentationMode.CircleWord));


			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractCubeCombined,
													Tesseract.PageSegmentationMode.SingleBlock));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractCubeCombined,
													Tesseract.PageSegmentationMode.SingleBlockVertText));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractCubeCombined,
													Tesseract.PageSegmentationMode.SingleChar));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractCubeCombined,
													Tesseract.PageSegmentationMode.SingleColumn));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractCubeCombined,
													Tesseract.PageSegmentationMode.SingleLine));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractCubeCombined,
													Tesseract.PageSegmentationMode.SingleWord));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractCubeCombined,
													Tesseract.PageSegmentationMode.SparseText));

			averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractCubeCombined,
													Tesseract.PageSegmentationMode.SparseTextOsd));

			Console.WriteLine("Done with all possible combinations of OcrengineModes and PagesegmentationModes");
*/
			foreach (TestData score in averageScores)
			{
				Console.WriteLine("\n\n\n ----------- Average score for config {0}, {1} is {2} -----------", score.engineName, score.segName, score.Average);
			}

			UserDialogs.Instance.HideLoading();
		}

		public async Task<TestData> GetAverageScore(UIImage[] inputImages, Tesseract.OcrEngineMode engineMode,
									   Tesseract.PageSegmentationMode segMode)
		{
			try
			{
				var correctStringOutputs = GetCorrectStringOutputs();
				List<int> successScores = new List<int>();
				double total = 0, average = 0;

				for (int c = 0; c < inputImages.Length; c++)
				{
					var image = GetStreamFromUIImage(inputImages[c]);
					successScores.Add(await ExtractTextTest(image, engineMode, 
					     segMode, correctStringOutputs[c], c));
				}

				foreach (int score in successScores)
				{
					total += score;
				}
				average = total / successScores.Count();

				return new TestData
				{
					Average = average,
					engineName = engineMode.ToString(),
					segName = segMode.ToString()
				};
			}
			catch (Exception e)
			{
				Console.WriteLine("Error in GetAverageScore: {0}", e.Message);
			}
			//Console.WriteLine("----------- AVERAGE SUCCESS SCORE: TesseractOnly, AutoOnly IS {0} ----------------", average);

			return new TestData
			{
				Average = 0,
				engineName = engineMode.ToString(),
				segName = segMode.ToString()
			};
		}

		public void RunTest(int imageCount)
		{
			var images = new List<UIImage>();
			int c = 0;

			try
			{
				for (; c < imageCount; c++)
				{
					images.Add(UIImage.FromFile("test" + c + ".jpg"));
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Error loading image resource #{0} reason: {1}", c, e.Message);
			}

			Console.WriteLine("Running Tesseract test on {0} images", images.Count);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is complete
			RunBusinessCardTest(images.ToArray());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is complete
		}

		public async Task<string> ExtractTextFromImage_Tesseract(Stream image, PageSegmentationMode pageMode, 
		                                                         OcrEngineMode engMode,
			Tesseract.PageIteratorLevel recognitionLevel = Tesseract.PageIteratorLevel.Textline)
		{ 
			Tesseract.Result[] imageResult = null;
			var text = string.Empty;
			TesseractApi api = new TesseractApi();
			try
			{
				if (await api.Init("eng")){
					api.SetPageSegmentationMode(pageMode);
					api.SetOcrEngineMode(engMode);
					api.MaximumRecognitionTime = TIMEOUTLIMIT / 1000;
					//Console.WriteLine("Running tesseract on config: {0}, {1}", pageMode, engMode);

					if (await api.SetImage(image))
						imageResult = api.Results(recognitionLevel).ToArray();//ToList();
					}

			}
			catch (Exception e)
			{
				System.Console.WriteLine("[ExtractTextFormImage_Tesseract] Error loading picture: " + e.Message);
			}

			if (imageResult == null)
			{
				UserDialogs.Instance.Alert("Couldn't read the image", "Pls try again", "OK");
			}

			Console.WriteLine("{0} lines in extracted text", imageResult.Length);
			for (int c = 0; c < imageResult.Length; c++) {
				Console.WriteLine("line {0}:{1}", c, imageResult[c].Text);
				text += imageResult[c].Text + "\n";
			}
			return text;
		}

		public async Task<List<Tesseract.Result>> ExtractResultFromImage_Tesseract(Stream image,
		    Tesseract.PageIteratorLevel recognitionLevel = Tesseract.PageIteratorLevel.Textline)
		{
			List<Tesseract.Result> imageResult = null;
			TesseractApi api = new TesseractApi();

			Console.WriteLine("api null? {0}", api == null);
			try
			{
				if (await api.Init("eng"))
				{
					api.SetPageSegmentationMode(PageSegmentationMode.Auto);
					api.SetOcrEngineMode(OcrEngineMode.TesseractCubeCombined);
					api.MaximumRecognitionTime = TIMEOUTLIMIT / 1000;

					if (await api.SetImage(image))
					{
						imageResult = api.Results(recognitionLevel).ToList();
					}
				}

			}
			catch (Exception e)
			{
				Console.WriteLine("[ExtractResultFromImage_Tesseract] Error loading picture: " + e.Message);
			}

			if (imageResult == null)
			{
				UserDialogs.Instance.Alert("Couldn't read the image", "Pls try again", "OK");
			}

			return imageResult;
		}

		public async Task<string> ExtractTextFromImage_ProjectOxford(Stream image)
		{
			Console.WriteLine("In ExtractTextFromImage_ProjectOxford");
			OcrResults result;
			string text = string.Empty;
			var client = new VisionServiceClient("53ffa1a4499e42caa902805119e297e2");

			result = await client.RecognizeTextAsync(image);

			foreach (var region in result.Regions)
			{
				foreach (var line in region.Lines)
				{
					text += "\n";
					Console.WriteLine("Regions.Lines.Tostring is {0}", line.ToString());
					foreach (var word in line.Words)
					{
						text += word.Text + " ";
					}
				}
			}
			return text;
		}

		public async Task<OcrResults> ExtractOCRResultFromImage_ProjectOxford(Stream image)
		{
			Console.WriteLine("In ExtractTextFromImage_ProjectOxford");
			try { 
				OcrResults result = new OcrResults();
				var client = new VisionServiceClient("53ffa1a4499e42caa902805119e297e2");

				result = await client.RecognizeTextAsync(image, "en");
				Console.WriteLine("language : {0}", result.Language);

				return result;
			} catch (Exception e) {
				Console.WriteLine("Microsoft OCR error: {0}", e.InnerException.Message);
			}
			return null;
		}

		public async void ReadBusinessCardThenSaveExportHandleTimeout_MicrosoftVision(UIImage image, UIProgressView progressBar,
			UIActivityIndicatorView loadingView)
		{
			UIImage preProcessedImage = null;
			/*Task.Factory.StartNew(async () =>
			{
				preProcessedImage = await PreprocessUIImage(image, false);//check sharpening quality
			});*/

			var cancelTokenSource = new CancellationTokenSource();
			var cancelToken = cancelTokenSource.Token;

			var readTask = Task.Factory.StartNew(() => {
				//ReadBusinessCardThenSaveExport_MicrosoftVision(
				//	GetStreamFromUIImage(image), progressBar, loadingView, cancelToken, true);

				Task.Run(() => { 
					for (int c = 0; !cancelToken.IsCancellationRequested; c++)
					{
						Console.WriteLine("do something do something do something");
						cancelToken.ThrowIfCancellationRequested();
						Console.WriteLine("CancelToken cancelled: {0}",
										  cancelToken.IsCancellationRequested);
					}
				}, cancelToken);
			}, cancelToken);
			//readTask.Start();
			while (readTask.Status != TaskStatus.Running)
			{
				//Wait until task is running
				Console.WriteLine("Not yet running");
			}

			try
			{
				for (int c = 0; c < 6; c++) {
					await Task.Delay(1000);
					Console.WriteLine("cancel countdown: {0}", c);
				}
				cancelTokenSource.Cancel();
				Console.WriteLine("cancelTokenSource.Cancel(): {0}, cancelToken: {1}", 
				                  cancelTokenSource.IsCancellationRequested, 
				                  cancelToken.IsCancellationRequested);
				/*for (int i = 0; i < TIMEOUTLIMIT / 1000; i++)
				{
					await Task.Delay(1000);
					if (i == (int)((TIMEOUTLIMIT / 1000) - 1)) cancelTokenSource.Cancel();
					Console.WriteLine("outside task Timing {1}... cancel? {0}",
									  cancelTokenSource.Token.IsCancellationRequested,
									 i);
				}*/
				/*
				if (cancelTokenSource.IsCancellationRequested)
				{
					Console.WriteLine(
							"After timeout: Cancellation requested: {0}, cancel Vision OCR? {1}, readTask cancelled? {2}",
								  cancelTokenSource.IsCancellationRequested,
									allowCancelMicrosoftVisionOCRTask, readTask.IsCanceled);
					//readTask.Dispose();
					//readTask = null;

					if (allowCancelMicrosoftVisionOCRTask)
					{
						if (preProcessedImage == null)
						{
							Console.WriteLine("Preprocessing not yet done, starting delay loop");
							for (int c = 0; c < 2; c++)
							{
								var exitLoop = false;
								await Task.Delay(500);
								if (preProcessedImage != null)
								{
									exitLoop = true;
									Console.WriteLine(
										"Preprocessing done, calling tesseract, exit loop {0}", exitLoop);
									GlobalVariables.VCToInvokeOnMainThread.InvokeOnMainThread(async () =>
										 await ReadBusinessCardThenSaveExport_Tesseract(preProcessedImage,
																		   progressBar, loadingView,
																				 false));
								}
								else Console.WriteLine("Preprocessing not yet done, looping");
								if (exitLoop) break;
							}
						}
						else GlobalVariables.VCToInvokeOnMainThread.InvokeOnMainThread(async () =>
										await ReadBusinessCardThenSaveExport_Tesseract(preProcessedImage,
																		  progressBar, loadingView,
																				false));
						if (preProcessedImage == null && allowCancelMicrosoftVisionOCRTask)
						{
							GlobalVariables.VCToInvokeOnMainThread.InvokeOnMainThread(() =>
							{
								UserDialogs.Instance.Alert("Image couldn't be read. Please try again",
														   "Oops", "OK");
							});
							ShowDoneLoading(progressBar, loadingView);
						}
					}
				}
				*/
			}
			catch (AggregateException e)
			{
				Console.WriteLine("Exception messages:");
				foreach (var ie in e.InnerExceptions)
					Console.WriteLine("   {0}: {1}", ie.GetType().Name, ie.Message);
			}
			catch (Exception e){
				Console.WriteLine("Exception: {0}", e.Message);
			}
			finally
			{
				cancelTokenSource.Dispose();
				Console.WriteLine("\nTask status: {0}, Task cancelled? {1}", readTask.Status, 
				                  readTask.IsCanceled);
			}
		}

		public UIImage ScaleImage(UIImage image, float maxDimension = 960){
			var scaledSize = new CGSize(maxDimension, maxDimension);
			nfloat scaleFactor = 0;

			if (image.Size.Width > image.Size.Height) {
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
			image.Draw(new CGRect(0,0,scaledSize.Width, scaledSize.Height));//(CGRectMake(0, 0, scaledSize.width, scaledSize.height))
			var scaledImage = UIGraphics.GetImageFromCurrentImageContext();//scaledImage = UIGraphicsGetImageFromCurrentImageContext()
			UIGraphics.EndImageContext();// UIGraphicsEndImageContext()

			Console.WriteLine("Scaled image to {0}, {1}", scaledImage.Size.Width, scaledImage.Size.Height);

			return scaledImage;
		}

		public async Task ReadBusinessCardThenSaveExport_Tesseract(UIImage image, UIProgressView progressBar, 
		                                                           UIActivityIndicatorView loadingView, bool preProcessImage = true)
		{
			Console.WriteLine("in ReadBusinessCardThenSaveExport_Tesseract");
			GlobalVariables.VCToInvokeOnMainThread.InvokeOnMainThread(
				() => UserDialogs.Instance.Alert("It'll be fine! The data usage is less then sending a text chat", "Use a data connection for way better text reading",
				                                 "Got it"));
			//UIImage preProcessedImage = null;
			ShowStartLoading(progressBar, loadingView);
			if (image == null) throw new ArgumentNullException("Illegal null value passed to ImagePreprocessor" +
															   ".ReadBusinessCardThenSaveExport_Tesseract(...)");
			ResetContactReaderData();
			int progressCtr = 0;

			//Preprocess image for better text recognition results - adds too much overhead time
			if(preProcessImage) image = await PreprocessUIImage(image, false, true);

			//save for testing purposes
			//SaveImageToPhotosApp(preProcessedStream, System.DateTime.Now.Second + "bwsharp.png");

			var result = await ExtractResultFromImage_Tesseract(GetStreamFromUIImage(image));
			var totalLines = (float)result.Count();
			var textline = string.Empty;
			var wholeTextForClipboard = string.Empty;

			foreach(var line in result)
			{
				Console.WriteLine("In lines");
				wholeTextForClipboard += "\n" + line.Text;
				textline = line.Text;
				progressCtr++;
				var readingProgress = progressBar.Progress + (float)(progressCtr / totalLines);
				progressBar.SetProgress(readingProgress, true);
				goToNextIteration = false;//reset

				//compare line to regex
				Console.WriteLine("about to process textline: nameFound? {0}", nameFound);
				if (!nameFound)
				{
					nameFound = IsName(line.Text);
					if (nameFound)
					{
						goToNextIteration = true;
						Console.WriteLine("Found name in text line, skipping to next line");
					}
					else goToNextIteration = false;
				}
				Console.WriteLine("Done checking for name, gotonextiteration {0}", goToNextIteration);
				if (goToNextIteration) continue;

				Console.WriteLine("about to process textline for number: numFound? {0}", numFound);
				numFound = IsNumber(line.Text);
				if (numFound)
				{
					goToNextIteration = true;
					Console.WriteLine("Found number in text line, skipping to next line");
				}
				else goToNextIteration = false;
				Console.WriteLine("Done checking for num, gotonextiteration {0}", goToNextIteration);
				if (goToNextIteration) continue;

				Console.WriteLine("about to process textline for org: orgFound? {0}", orgFound);
				if (!orgFound)
				{
					orgFound = IsOrg(line.Text);
					if (orgFound)
					{
						goToNextIteration = true;
						Console.WriteLine("Found org in text line, skipping to next line");
					}
					else goToNextIteration = false;
				}
				Console.WriteLine("Done checking for org, gotonextiteration {0}", goToNextIteration);
				if (goToNextIteration) continue;

				Console.WriteLine("about to process textline for EMAIL: emailFound? {0}", emailFound);
				if (!emailFound)
				{
					emailFound = IsEmail(line.Text);
					if (emailFound)
					{
						goToNextIteration = true;
						Console.WriteLine("Found email in text line, skipping to next line");
					}
					else goToNextIteration = false;
				}
				Console.WriteLine("Done checking for email, gotonextiteration {0}", goToNextIteration);
				if (goToNextIteration) continue;

				Console.WriteLine("about to process textline for URL: URLFound? {0}", URLFound);
				if (!URLFound)
				{
					URLFound = IsURL(line.Text);
					if (URLFound)
					{
						goToNextIteration = true;
						Console.WriteLine("Found URL in text line, skipping to next line");
					}
					else goToNextIteration = false;
				}
				Console.WriteLine("Done checking for URL, gotonextiteration {0}", goToNextIteration);
				if (goToNextIteration) continue;

				CheckAddress(line.Text);

				Console.WriteLine("about to process textline: jobFound? {0}", jobFound);
				if (!jobFound)
				{
					jobFound = IsJob(line.Text);
					if (jobFound)
					{
						goToNextIteration = true;
						Console.WriteLine("Found job in text line, skipping to next line");
					}
					else goToNextIteration = false;
				}
				Console.WriteLine("Done checking for job, gotonextiteration {0}", goToNextIteration);
				if (goToNextIteration) continue;
			}

			//for some reason, only first call to SaveAddressToContact() actually saves to CNPostalAddress, 
			//so call after loop for now
			SaveAddressToContact();

			ShowDoneLoading(progressBar, loadingView);
			GlobalVariables.VCToInvokeOnMainThread.InvokeOnMainThread(
					() => PostImageRecognitionActions.OpenIn(contact, wholeTextForClipboard));
		}

		void ShowStartLoading(UIProgressView progressBar, UIActivityIndicatorView loadingView)
		{
			GlobalVariables.VCToInvokeOnMainThread.InvokeOnMainThread(() => { 
				progressBar.Hidden = false;
				loadingView.StartAnimating();
				progressBar.SetProgress(0.25f, true);
			});
		}
		async void ShowDoneLoading(UIProgressView progressBar, UIActivityIndicatorView loadingView) {
			GlobalVariables.VCToInvokeOnMainThread.InvokeOnMainThread(async () => { 
				progressBar.SetProgress(1.0f, true);
				await Task.Delay(1000);
				progressBar.Hidden = true;
				loadingView.StopAnimating();
				progressBar.SetProgress(0.0f, false);
			});
		}

		public async void ReadBusinessCardThenSaveExport_MicrosoftVision(Stream image, UIProgressView progressBar, 
		    UIActivityIndicatorView loadingView, CancellationToken cancelToken, bool canTimeout = false)
		{
			try
			{
				allowCancelMicrosoftVisionOCRTask = true;
				ShowStartLoading(progressBar, loadingView);

				if (image == null) throw new ArgumentNullException("Illegal null value passed to ImagePreprocessor" +
																   ".ReadBusinessCardThenSaveExport(stream)");
				OcrResults result = null;
				ResetContactReaderData();
				int progressCtr = 0;
				string wholeTextForClipboard = "";

				image = GetStreamFromUIImage(ScaleImage(GetUIImageFromStream(image)));

				if (canTimeout)
				{
					Task.Run(async () =>
					{
						try
						{
							for (int i = 0; i < TIMEOUTLIMIT / 1000; i++)
							{
								await Task.Delay(1000);
								cancelToken.ThrowIfCancellationRequested();
								Console.WriteLine("in task Timing {1}... cancel? {0}",
												  cancelToken.IsCancellationRequested,
												 i);
							}
						}
						catch (OperationCanceledException oe)
						{
							Console.WriteLine("OperationCanceledException: {0}", oe.Message);
						}
						catch (Exception e){
							Console.WriteLine("canTimeout Exception: {0}", e.Message);
						}
					});
				}

				result = await ExtractOCRResultFromImage_ProjectOxford(image);
				allowCancelMicrosoftVisionOCRTask = false;
				Console.WriteLine("cancel Vision OCR? {0}", allowCancelMicrosoftVisionOCRTask);
				GlobalVariables.VCToInvokeOnMainThread.InvokeOnMainThread(
					() => progressBar.SetProgress(0.5f, true));

				foreach (var region in result.Regions)
				{
					Console.WriteLine("In regions");
					foreach (var line in region.Lines)
					{
						Console.WriteLine("In lines");
						wholeTextForClipboard += "\n" + CombineWordsIntoLine(line);
						progressCtr++;
						GlobalVariables.VCToInvokeOnMainThread.InvokeOnMainThread(() =>
						{
							var readingProgress = progressBar.Progress + (float)(progressCtr / (float)region.Lines.Count());
							progressBar.SetProgress(readingProgress, true);
						});
						goToNextIteration = false;//reset

						//compare line to regex
						//Console.WriteLine("about to process textline: nameFound? {0}", nameFound);
						if (!nameFound)
						{
							nameFound = IsName(CombineWordsIntoLine(line));
							if (nameFound)
							{
								goToNextIteration = true;
								//Console.WriteLine("Found name in text line, skipping to next line");
							}
							else goToNextIteration = false;
						}
						//Console.WriteLine("Done checking for name, gotonextiteration {0}", goToNextIteration);
						if (goToNextIteration) continue;

						//Console.WriteLine("about to process textline for number: numFound? {0}", numFound);
						numFound = IsNumber(CombineWordsIntoLine(line));
						if (numFound)
						{
							goToNextIteration = true;
							//Console.WriteLine("Found number in text line, skipping to next line");
						}
						else goToNextIteration = false;
						//Console.WriteLine("Done checking for num, gotonextiteration {0}", goToNextIteration);
						if (goToNextIteration) continue;

						//Console.WriteLine("about to process textline for org: orgFound? {0}", orgFound);
						if (!orgFound)
						{
							orgFound = IsOrg(CombineWordsIntoLine(line));
							if (orgFound)
							{
								goToNextIteration = true;
								//Console.WriteLine("Found org in text line, skipping to next line");
							}
							else goToNextIteration = false;
						}
						//Console.WriteLine("Done checking for org, gotonextiteration {0}", goToNextIteration);
						if (goToNextIteration) continue;

						//Console.WriteLine("about to process textline for EMAIL: emailFound? {0}", emailFound);
						if (!emailFound)
						{
							emailFound = IsEmail(CombineWordsIntoLine(line));
							if (emailFound)
							{
								goToNextIteration = true;
								//Console.WriteLine("Found email in text line, skipping to next line");
							}
							else goToNextIteration = false;
						}
						//Console.WriteLine("Done checking for email, gotonextiteration {0}", goToNextIteration);
						if (goToNextIteration) continue;

						//Console.WriteLine("about to process textline for URL: URLFound? {0}", URLFound);
						if (!URLFound)
						{
							URLFound = IsURL(CombineWordsIntoLine(line));
							if (URLFound)
							{
								goToNextIteration = true;
								//Console.WriteLine("Found URL in text line, skipping to next line");
							}
							else goToNextIteration = false;
						}
						//Console.WriteLine("Done checking for URL, gotonextiteration {0}", goToNextIteration);
						if (goToNextIteration) continue;

						CheckAddress(CombineWordsIntoLine(line));

						//Console.WriteLine("about to process textline: jobFound? {0}", jobFound);
						if (!jobFound)
						{
							jobFound = IsJob(CombineWordsIntoLine(line));
							if (jobFound)
							{
								goToNextIteration = true;
								//Console.WriteLine("Found job in text line, skipping to next line");
							}
							else goToNextIteration = false;
						}
						//Console.WriteLine("Done checking for job, gotonextiteration {0}", goToNextIteration);
						if (goToNextIteration) continue;
					}
				}

				//for some reason, only first call to SaveAddressToContact() actually saves to CNPostalAddress, 
				//so call after loop for now
				SaveAddressToContact();

				ShowDoneLoading(progressBar, loadingView);
				GlobalVariables.VCToInvokeOnMainThread.InvokeOnMainThread(
					() => PostImageRecognitionActions.OpenIn(contact, wholeTextForClipboard));
			}catch(AggregateException ae){
				Console.WriteLine("ReadBusinessCardThenSaveExport_MicrosoftVision error: {0}",
				                  ae.Message);
			} catch (Exception ex){
				Console.WriteLine("ReadBusinessCardThenSaveExport_MicrosoftVision(...) error: {0}", 
				                  ex.Message);
			}
		}

		bool IsOrg(string input) {
			//input = RegexHelper.RemoveCommonTesseractURLErrors(input);
			Console.WriteLine("Org: input filtered errors and special chars:{0}", input);

			var orgRegex = new Regex(RegexHelper.ORGREGEX);
			var orgMatch = orgRegex.Match(input);

			if (orgMatch.Success){
				contact.OrganizationName += " " + RemoveLabel(input);
				Console.WriteLine("Found org name: {0}", contact.OrganizationName);
				return true;
			}
			Console.WriteLine("No org found in textline");
			return false;
		}



		bool IsStreet(string input) {
			//extract Street address
			input = RemoveLabel(input);

			if (HasStreetString(input)) return true;

			var streetRegex = new Regex(RegexHelper.STREETREGEX);
			var streetMatch = streetRegex.Match(input);

			if (streetMatch.Success)
			{
				var street = streetMatch.Groups[0].Value;
				//var mutableContact = contact.MutableCopy() as CNMutableContact;
				//var address = mutableContact.PostalAddresses[0].MutableCopy() as CNMutablePostalAddress;
				//address.Street = street;
				postalAddress.Street = street;
				//input = input.Replace(street, string.Empty);
				Console.WriteLine("Found single match street: {0}", street);
				return true;
			}
			Console.WriteLine("No single match street found");
			return false;
		}
		bool ContainsCityLabel(string input) { 
			var cityLabels = RegexHelper.CITYLABELS.Split(new char[] { ' ', '\t' });
			for (int c = 0; c < cityLabels.Length; c++)
			{
				if (input.ToLower().Contains(cityLabels[c].ToLower()))
					return true;
			}
			return false;
		}
		bool IsCity(string input)
		{
			//input = RemoveLabel(input);
			//var cityRegex = new Regex(RegexHelper.CITYREGEX);
			//var cityMatch = cityRegex.Match(input);
			Console.WriteLine("checking for city labels: {0}", input);
			if (ContainsCityLabel(input))//cityMatch.Success)
			{
				Console.WriteLine("Found city labels: {0}", input);
				//var city = cityMatch.Groups[0].Value;
				//var address = contact.PostalAddresses[0].Value.MutableCopy() as CNMutablePostalAddress;
				//address.City = city;
				//contact.PostalAddresses[0] = new CNLabeledValue<CNPostalAddress>("work", (CNPostalAddress)address);

				postalAddress.City = input;

				//input = input.Replace(city, string.Empty);
				Console.WriteLine("Found single match city: {0}", postalAddress.City);
				return true;
			}
			//else Console.WriteLine("No single match city found");
			return false;
		}
		bool IsPostalCode(string input)
		{
			input = RemoveLabel(input);
			var postalRegex = new Regex(RegexHelper.POSTALREGEX);
			var postalMatch = postalRegex.Match(input);

			if (postalMatch.Success)
			{
				var postal = postalMatch.Groups[0].Value;
				postalAddress.PostalCode = postal;
				input = input.Replace(postal, string.Empty);
				Console.WriteLine("Found single match postal: {0}", postal);
				return true;
			}
			else Console.WriteLine("No single match postal found");
			return false;
		}

		bool IsCityAndPostal(string input) { 
			input = RemoveLabel(input);

			Console.WriteLine("IsCityAndPostal: checking for city labels: {0}", input);
			if (ContainsCityLabel(input))
			{
				Console.WriteLine("IsCityAndPostal: Found city labels: {0}", input);
				var postalRegex = new Regex(RegexHelper.POSTALREGEX);
				var postalMatch = postalRegex.Match(input);

				if (postalMatch.Success) {
					Console.WriteLine("IsCityAndPostal: Found postal code: {0}", postalMatch.Groups[0].Value);
					postalAddress.PostalCode = postalMatch.Groups[0].Value;
					return true;
				}
			}
			//else Console.WriteLine("IsCityAndPostal: No single match citypostal found");
			return false;

			/*var citypostalRegex = new Regex(RegexHelper.CITYPOSTALREGEX);
			var citypostalMatch = citypostalRegex.Match(input);
			if (citypostalMatch.Success)
			{
				var postalRegex = new Regex(RegexHelper.POSTALREGEX);
				var postalMatch = postalRegex.Match(input);

				if (postalMatch.Success)
				{
					var postal = postalMatch.Groups[0].Value;
					postalAddress.PostalCode = postal;
					input = input.Replace(postal, string.Empty);
					Console.WriteLine("Found postal: {0}, remaining string: {1}", postal, input);

				}

				Console.WriteLine("Found postal");
				return true;
			}
			else Console.WriteLine("No single match citypostal found");
			return false;*/
		}

		void SaveAddressToContact() {
			//try {
				var addresses = contact.PostalAddresses.ToList();
				addresses.Add(new CNLabeledValue<CNPostalAddress>("work", (CNPostalAddress)postalAddress));
				contact.PostalAddresses = new CNLabeledValue<CNPostalAddress>[]{};
				contact.PostalAddresses = addresses.ToArray();

				Console.WriteLine("postalAddress street and city: {0}, {1}", postalAddress.Street, postalAddress.City);
				Console.WriteLine("contact postaladdress field exists: {0},{1}", contact.PostalAddresses[0].Value.Street,
				                 contact.PostalAddresses[0].Value.City);
			/*} catch (IndexOutOfRangeException e){
				Console.WriteLine("contact postaladdress field doesnt exist yet, creating...");

			}*/

			if (streetFound || cityFound) Console.WriteLine("Found address: {0},{1}",
															postalAddress.Street, postalAddress.City);
		}

		void CheckAddress(string input)
		{
			Console.WriteLine("Address: input filtered errors and special chars:{0}", input);

			var addressRegex = new Regex(RegexHelper.ADDRESSREGEX);
			//var addressMatch = addressRegex.Match(input);
			//string addressWithoutLabel = input;

			Console.WriteLine("streetFound? {0}", streetFound);
			if (!streetFound)
			{
				streetFound = IsStreet(input);
				if (streetFound)
				{
					Console.WriteLine("Found street, skipping to next line");
					return;
				}
				else Console.WriteLine("No street found");
			}
			Console.WriteLine("cityFound? {0}", cityFound);
			if (!cityFound)
			{
				cityFound = IsCity(input);
				if (cityFound) { 
					Console.WriteLine("Found city, skipping to next line");
					return;
				}else Console.WriteLine("No city found");
			}
			Console.WriteLine("citypostalFound? {0}", citypostalFound);
			if (!citypostalFound)
			{
				citypostalFound = IsCityAndPostal(input);
				if (citypostalFound)
				{
					Console.WriteLine("Found citypostal, skipping to next line");
					return;
				}
				else Console.WriteLine("No citypostal found");
			}

			/*if (addressMatch.Success)
			{
				Console.WriteLine("Found an address: {0}", addressMatch.Groups[0].Value);

				addressWithoutLabel = RemoveLabel(input);

				//extract Street address
				var streetRegex = new Regex(RegexHelper.STREETREGEX);
				var streetMatch = streetRegex.Match(addressWithoutLabel);
				if (streetMatch.Success){
					var street = streetMatch.Groups[0].Value;
					postalAddress.Street = street;
					addressWithoutLabel = addressWithoutLabel.Replace(street, string.Empty);
					Console.WriteLine("Found street: {0}, remaining string: {1}", street, addressWithoutLabel);
				} else Console.WriteLine("No street found, remaining string: {0}", addressWithoutLabel);*/

			/*var citypostalRegex = new Regex(RegexHelper.CITYPOSTALREGEX);
			var citypostalMatch = citypostalRegex.Match(addressWithoutLabel);
			if (citypostalMatch.Success)
			{
				Console.WriteLine("Found citypostal match: {0}", citypostalMatch.Groups[0].Value);
				var postalRegex = new Regex(RegexHelper.POSTALREGEX);
				var postalMatch = postalRegex.Match(addressWithoutLabel);
				var cityRegex = new Regex(RegexHelper.CITYREGEX);
				var cityMatch = cityRegex.Match(addressWithoutLabel);

				if (postalMatch.Success)
				{
					var postal = postalMatch.Groups[0].Value;
					postalAddress.PostalCode = postal;
					addressWithoutLabel = addressWithoutLabel.Replace(postal, string.Empty);
					Console.WriteLine("Found postal: {0}, remaining string: {1}", postal, addressWithoutLabel);

					if (cityMatch.Success)
					{
						var city = cityMatch.Groups[0].Value;
						postalAddress.City = city;
						addressWithoutLabel = addressWithoutLabel.Replace(city, string.Empty);
						Console.WriteLine("Found city: {0}, remaining string: {1}", city, addressWithoutLabel);
					}

				}
				else if (cityMatch.Success)
				{
					var city = cityMatch.Groups[0].Value;
					postalAddress.City = city;
					addressWithoutLabel = addressWithoutLabel.Replace(city, string.Empty);
					Console.WriteLine("Found city: {0}, remaining string: {1}", city, addressWithoutLabel);

					if (postalMatch.Success)
					{
						var postal = postalMatch.Groups[0].Value;
						postalAddress.PostalCode = postal;
						addressWithoutLabel = addressWithoutLabel.Replace(postal, string.Empty);
						Console.WriteLine("Found postal: {0}, remaining string: {1}", postal, addressWithoutLabel);
					}
				}
			}*/

			//extract City
				/*var cityRegex = new Regex(RegexHelper.CITYREGEX);
				var cityMatch = cityRegex.Match(addressWithoutLabel);
				if (cityMatch.Success)
				{
					var city = cityMatch.Groups[0].Value;
					postalAddress.City = city;
					addressWithoutLabel = addressWithoutLabel.Replace(city, string.Empty);
					Console.WriteLine("Found city: {0}, remaining string: {1}", city, addressWithoutLabel);
				} else Console.WriteLine("No city found, remaining string: {0}", addressWithoutLabel);*/ 

				//extract Country
				/*var countryRegex = new Regex(RegexHelper.COUNTRYREGEX);
				var countryMatch = countryRegex.Match(addressWithoutLabel);
				if (countryMatch.Success)
				{
					var country = countryMatch.Groups[0].Value;
					postalAddress.Country = country;
					addressWithoutLabel = addressWithoutLabel.Replace(country, string.Empty);
					Console.WriteLine("Found country: {0}, remaining string: {1}", country, addressWithoutLabel);
				} else Console.WriteLine("No country found, remaining string: {0}", addressWithoutLabel); */

				//extract ZIP/Postal Code
				/*var postalRegex = new Regex(RegexHelper.POSTALREGEX);
				var postalMatch = postalRegex.Match(addressWithoutLabel);
				if (postalMatch.Success)
				{
					var postal = postalMatch.Groups[0].Value;
					postalAddress.PostalCode = postal;
					addressWithoutLabel = addressWithoutLabel.Replace(postal, string.Empty);
					Console.WriteLine("Found postal: {0}, remaining string: {1}", postal, addressWithoutLabel);
				} else Console.WriteLine("No postal found, remaining string: {0}", addressWithoutLabel);

				addresses.Add(new CNLabeledValue<CNPostalAddress>("work", (CNPostalAddress)postalAddress));
				contact.PostalAddresses = addresses.ToArray();
				if(streetFound || cityFound) Console.WriteLine("Found address: {0}, {1}", 
				                                               postalAddress.Street, postalAddress.City);
				return true;
			}
			Console.WriteLine("No address found in textline");
			return false;*/
		}

		bool IsPosition() {
			throw new NotImplementedException();
		}

		string CorrectWWWURLOCRError(string input) {
			var wwwInputString = input;
			if (!wwwInputString.Contains(RegexHelper.WWWDOTSTRING) &&
				wwwInputString.Contains(RegexHelper.WWWSTRING))
			{
				Console.WriteLine("{0} contains www error", wwwInputString);
				input = wwwInputString.Replace(RegexHelper.WWWSTRING, RegexHelper.WWWDOTSTRING);
				Console.WriteLine("Fixed www error: {0}", input);
			}
			else Console.WriteLine("No www error found");
			return input;
		}

		bool IsURL(string input) { 
			//input = //RegexHelper.RemoveCountryCodeReadingErrorsAndSpecialChar(
				//RegexHelper.RemoveCommonTesseractURLErrors(input);//);
			Console.WriteLine("URL: input filtered errors and special chars:{0}", input);

			var urlRegex = new Regex(RegexHelper.WWW_LIMITEDURLREGEX);
			var urlMatch = urlRegex.Match(input);

			//for (int c = 0; c < input.Length; c++)
			//{
				if (urlMatch.Success)
				{
					var urlLabeledMatchString = urlMatch.Groups[0].Value;

					var labelReg = new Regex(RegexHelper.LABELREGEX);
					Match labelMatch = labelReg.Match(urlLabeledMatchString);

					//remove  label if any
					if (labelMatch.Success)
					{
						Console.WriteLine("found url label: {0}", labelMatch.Groups[0].Value);
						try
						{
							var temp = urlLabeledMatchString.Replace(labelMatch.Groups[0].Value, string.Empty);
							if (!string.IsNullOrWhiteSpace(temp)) urlLabeledMatchString = temp;
						}
						catch (Exception e)
						{
							Console.WriteLine("URL error: {0}", e.Message);
						}
						Console.WriteLine("Removed url label: {0}", urlLabeledMatchString);
					}

					//check for www. error (missing '.')
					urlLabeledMatchString = CorrectWWWURLOCRError(urlLabeledMatchString);

					var urls = contact.UrlAddresses.ToList();
					urls.Add(new CNLabeledValue<NSString>("Website", (NSString)urlLabeledMatchString));
					contact.UrlAddresses = urls.ToArray();
				
					return true;
				}
				else Console.WriteLine("No URL Found in this line");
			//}
			return false;
		}

		bool IsEmail(string input) { 
			//input = //RegexHelper.RemoveCountryCodeReadingErrorsAndSpecialChar(
				//RegexHelper.RemoveCommonTesseractEmailErrors(input);//);
			Console.WriteLine("input filtered errors and special chars:{0}", input);

			var emailRegex = new Regex(RegexHelper.LABELEDEMAILREGEX);
			var emailMatch = emailRegex.Match(input);

			if (emailMatch.Success)
			{
				var labeledEmailMatchString = emailMatch.Groups[0].Value;
				var labelReg = new Regex(RegexHelper.LABELREGEX);
				Match labelMatch = labelReg.Match(labeledEmailMatchString);

				//remove  label if any
				if (labelMatch.Success)
				{
					Console.WriteLine("found email label: {0}", labelMatch.Groups[0].Value);
					try
					{
						var temp = labeledEmailMatchString.Replace(labelMatch.Groups[0].Value, string.Empty);
						if (!string.IsNullOrWhiteSpace(temp)) labeledEmailMatchString = temp;
					}
					catch (Exception e)
					{
						Console.WriteLine("IsEmail error: {0}", e.Message);
					}
					Console.WriteLine("Removed email label: {0}", labeledEmailMatchString);
				}

				var emails = contact.EmailAddresses.ToList();
				emails.Add(new CNLabeledValue<NSString>("Email", (NSString)labeledEmailMatchString));
				contact.EmailAddresses = emails.ToArray();

				return true;
			}
			else Console.WriteLine("No email found");
			return false;
		}
		bool IsJob(string input) { 
			if (HasJobTitle(input))
			{
				contact.JobTitle = input;
				jobFound = true;
				return true;
			}
			return false;
		}
		bool IsName(string input) { 
			string[] names;
			var NameRegex = new Regex(RegexHelper.PERSONSNAMEREGEX);
			Match nameMatch = NameRegex.Match(
				RegexHelper.RemoveCountryCodeReadingErrorsAndSpecialChar(
					input));

			Console.WriteLine("input is {0}", input);

			if (nameMatch.Success)
			{
				if (HasCompanySuffix(nameMatch.Groups[0].Value)) return false;
				if (IsJob(nameMatch.Groups[0].Value)) return false;
				if (HasStreetString(nameMatch.Groups[0].Value)) return false;

				Console.WriteLine("name MATCH: " + nameMatch.Groups[0].Value);
				names = nameMatch.Groups[0].Value.Split(new char[] { ' ', '\t' });
				Console.WriteLine("names split by spaces count: " + names.Length);
				contact.GivenName = names[0];
				for (int i = 1; i < names.Length; i++)
				{
					contact.FamilyName += " " + names[i];
				}
				Console.WriteLine("Name found: {0} {1}", contact.GivenName, contact.FamilyName);
				return true;
			}
			Console.WriteLine("No name found in this line");
			return false;
		}
		bool HasJobTitle(string input) {
			var jobs = RegexHelper.JOBTITLES.Split(new char[] { ' ', '\t' });
			Console.WriteLine("{0} job title strings in memory", jobs.Length);
			for (int c = 0; c < jobs.Length; c++) 
			{
				Console.WriteLine("string {0} is {1}", c, jobs[c]);
				if (input.ToLower().Contains(jobs[c].ToLower()))
				{
					Console.WriteLine("Found jobs match: {0}", jobs[c]);
					return true;
				}
			}
			return false;
		}

		bool HasStreetString(string input)
		{
			var streets = RegexHelper.STREETLABELS.Split(new char[] { ' ', '\t' });
			Console.WriteLine("{0} street title strings in memory", streets.Length);
			for (int c = 0; c < streets.Length; c++)
			{
				Console.WriteLine("string {0} is {1}", c, streets[c]);
				if (input.ToLower().Contains(streets[c].ToLower()))
				{
					Console.WriteLine("Found street match: {0}", streets[c]);
					postalAddress.Street = input;
					streetFound = true;
					return true;
				}
			}
			return false;
		}

		bool HasCompanySuffix(string input) { 
			var companyStrings = RegexHelper.COMPANYSTRINGS.Split(new char[] { ' ', '\t' });
			Console.WriteLine("{0} company strings in memory", companyStrings.Length);
			for (int c = 0; c < companyStrings.Length; c++)
			{
				Console.WriteLine("string {0} is {1}", c, companyStrings[c]);
				if (input.ToLower().Contains(companyStrings[c].ToLower()))
				{
					Console.WriteLine("Found company match: {0}", companyStrings[c]);
					contact.OrganizationName += input;
					orgFound = true;
					return true;
				}
			}
			return false;
		}
		bool IsNumber(string input) {
			var numReg = new Regex(RegexHelper.MULTILABELEDNUMREGEX);
			input = RegexHelper.RemoveCountryCodeReadingErrorsAndSpecialChar(input);
			input = input.Replace(" ", "");
			Console.WriteLine("filtered input: {0}", input);
			var numMatch = numReg.Match(input);

			var strictNumRegex = new Regex(RegexHelper.STRICTNUMREGEX);
			string strictMatchString = string.Empty;
			
			if (numMatch.Success){
				var number = string.Empty;
				var rawMatchString = numMatch.Groups[0].Value;
				Console.WriteLine("Found labeled number match: {0}", rawMatchString);

				var numLabelReg = new Regex(RegexHelper.LABELREGEX);
				Match labelMatch = numLabelReg.Match(rawMatchString);

				if (labelMatch.Success)
				{
					Console.WriteLine("found number label: {0}", labelMatch.Groups[0].Value);
					try
					{
						var temp = rawMatchString.Replace(labelMatch.Groups[0].Value, string.Empty);
						if (!string.IsNullOrWhiteSpace(temp)) rawMatchString = temp;
					}
					catch (Exception e)
					{
						Console.WriteLine("IsName error: {0}", e.Message);
					}
					Console.WriteLine("Removed number label: {0}", rawMatchString);
				}

				Match strictMatch = strictNumRegex.Match(rawMatchString);
				if (strictMatch.Success)
				{
					strictMatchString = strictMatch.Groups[0].Value;
					Console.WriteLine("EXACT NUMBER MATCH: " + strictMatchString);

					//remove spaces
					string[] num = strictMatchString.Split(new char[] { ' ', '\t' });
					Console.WriteLine("number split by spaces count: " + num.Length);
					if (num.Length > 1)
					{
						for (int i = 0; i < num.Length; i++)
						{
							number += num[i];
						}
					}
					else {
						number = strictMatchString;
					}

					Console.WriteLine("extracted number is {0}", number);
				}

				Console.WriteLine("Number match is {0}", numMatch.Groups[0].Value);
				var numbers = contact.PhoneNumbers.ToList();
				numbers.Add(new CNLabeledValue<CNPhoneNumber>("mobile",
					  new CNPhoneNumber(number)));
				contact.PhoneNumbers = numbers.ToArray();
				return true;
			}
			Console.WriteLine("No number match");
			return false;
		}

		string RemoveLabel(string input) { 
			var labelRegex = new Regex(RegexHelper.LABELREGEX);
			Match labelMatch = labelRegex.Match(input);
			string labelMatchString = input;

			//remove  label if any
			if (labelMatch.Success)
			{
				Console.WriteLine("found label: {0}", labelMatch.Groups[0].Value);
				try
				{
					var temp = labelMatchString.Replace(labelMatch.Groups[0].Value, string.Empty);
					if (!string.IsNullOrWhiteSpace(temp)) labelMatchString = temp;
				}
				catch (Exception e)
				{
					Console.WriteLine("removelabel error: {0}", e.Message);
				}
				Console.WriteLine("Removed label: {0}", labelMatchString);
			}
			return labelMatchString;
		}

		string CombineWordsIntoLine(Line textline) {
			string lineText = string.Empty;
			for (int c = 0; c < textline.Words.Length; c++){
				Console.WriteLine("word found: {0}", textline.Words[c].Text);
				lineText += " " + textline.Words[c].Text;
			}
			return lineText;
		}
		string[] SeparateTesseractResultIntoStringLines(string input) { 
			return input.Split(new char[] { '\n' });
		}

		/*public async Task ReadAnyBusinessCardThenSaveOrExport(Stream bwSharpenedStream) {
			//check line: is it a name, number, company, url, etc - then call the matching function
			CNMutableContact contact = new CNMutableContact();
			bool contactFound = false, numberFound = false, emailFound = false, urlFound = false;

			var extractedText = await ExtractTextFromImage(bwSharpenedStream, 
			    Tesseract.PageIteratorLevel.Textline);
			
			for (int c = 0; c < extractedText.Length; c++) {
				var extractedTextLine = extractedText[c].Text;
				Console.WriteLine("confidence: {3} - extracted text block iteration {0}/{2} content:{1}", 
				                  c, extractedTextLine, extractedText.Length, extractedText[c].Confidence);

				MatchFormat matches = null;
				IfObjectNullExecuteAction(matches, async () => {
					if (!contactFound) { 
							matches =
								await ReadAnyNameReturnMatchAndRemainderString(extractedTextLine);

						if (matches != null)
						{
							var arr = matches.Matches.ToArray();
							contact.GivenName = arr[0];
							for (int n = 1; n < arr.Length; n++)
							{
								contact.FamilyName += arr[n];
							}
							contactFound = true;
						}
						else Console.WriteLine("No name found in this textline");
					}
				});

				IfObjectNullExecuteAction(matches, async () => {
					if (!numberFound) { 
							matches =
								await ReadAnyPhoneNumReturnMatchAndRemainderString(extractedTextLine);

						if (matches != null)
						{
							var arr = matches.Matches;
							var numbers = contact.PhoneNumbers.ToList();
							for (int n = 0; n < arr.Length; n++)
							{
								numbers.Add(new CNLabeledValue<CNPhoneNumber>("mobile",
							        new CNPhoneNumber(matches.Matches[n])));
							}
							contact.PhoneNumbers = numbers.ToArray();
							numberFound = true;
						}
						else Console.WriteLine("No number found in this textline");
					}
				});

				IfObjectNullExecuteAction(matches, async () => {
					if (!emailFound) {
						matches = await ReadAnyEmailReturnMatchAndRemainderString(extractedTextLine);

						if (matches != null)
						{
							var emails = contact.EmailAddresses.ToList();
							var arr = matches.Matches;
							for (int n = 0; n < arr.Length; n++)
							{
								emails.Add(new CNLabeledValue<NSString>("", (NSString)arr[n]));
							}
							contact.EmailAddresses = emails.ToArray();
							emailFound = true;
						}
						else Console.WriteLine("No email found in this textline");
					}
				});

				IfObjectNullExecuteAction(matches, async () =>
				{
					if (!urlFound){
						matches = await ReadAnyURLReturnMatchAndRemainderString(extractedTextLine);

						if (matches != null)
						{
							var urls = contact.UrlAddresses.ToList();
							var arr = matches.Matches;
							for (int n = 0; n < arr.Length; n++)
							{
								urls.Add(new CNLabeledValue<NSString>("", (NSString)arr[n]));
							}
							contact.UrlAddresses = urls.ToArray();
							urlFound = true;
						}
						else Console.WriteLine("No urls found in this textline");
					}
				});
			}

			PostImageRecognitionActions.OpenIn(contact);
		}
		void IfObjectNullExecuteAction(object match, Action recognitionAction) {
			if (match == null) recognitionAction();
		}*/

		/*public string SaveImageToDiskAndPhotosThenReturnPath(Stream s, string filename)
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
		}*/

		public void SaveImageToPhotosApp(UIImage someImage, string filename){
			try{
				someImage.SaveToPhotosAlbum((image, error) => {
					var o = image as UIImage;
				});
			}catch(Exception e){
				Console.WriteLine ("error saving processed image: {0}", e.Message);
			}
		}

		public async Task<UIImage> PreprocessUIImage(UIImage transformedcroppedimage, 
		                                             bool saveImage = false, bool useGPU = false)
		{
			Console.WriteLine("in PreprocessImage");

			if (transformedcroppedimage != null) {
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
						() => SaveImageToPhotosApp(finalImage, System.DateTime.Now.Second + "bwsharp.png"));
				}

				Console.WriteLine("Done preprocessing");
				return finalImage;
			}
			Console.WriteLine("Failed preprocessing");
			return null;
		}

		public UIImage AdaptiveThreshold(UIImage inputImage) {
			if (inputImage != null) {
				//var greyScale = new GPUImageGrayscaleFilter();
				var imageFilter = new GPUImageAdaptiveThresholdFilter {BlurRadiusInPixels = 5 };
				return imageFilter.CreateFilteredImage(/*greyScale.CreateFilteredImage(*/inputImage);//));
			}
			return null;
		}

		public UIImage ApplyGreyScale(UIImage image) { 
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

		UIImage UnSharpMask(UIImage origImage, bool useGPU = false){
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


		public Stream GetStreamFromFilename(string file){
			//Console.WriteLine ("In GetStreamFromFilename");
			return GetStreamFromUIImage (UIImage.FromFile (file));
		}
		public Stream GetStreamFromUIImage(UIImage image){
			//Console.WriteLine ("In GetStreamFromUIImage");
			return BytesToStream(UIImageToBytes (image));
		}
		public UIImage GetUIImageFromStream(Stream s){
			//Console.WriteLine ("in GetUIImageFromStream");
			return GetImagefromByteArray(StreamToBytes (s));
		}

		public UIImage GetImagefromByteArray (byte[] imageBuffer)
		{
			//Console.WriteLine ("in GetImagefromByteArray");
			NSData imageData = NSData.FromArray(imageBuffer);
			//Console.WriteLine ("NSData loaded from bytes");
			var img = UIImage.LoadFromData (imageData);
			//Console.WriteLine ("UIImage null: {0}", (img == null) ? true : false);
			return img;
		}

		public byte[] StreamToBytes(Stream input)
		{
			//Console.WriteLine ("In StreamToBytes");
			using (MemoryStream ms = new MemoryStream()){
				input.CopyTo(ms);
				//Console.WriteLine ("bytes copied");
				ms.Seek(0, SeekOrigin.Begin);
				//Console.WriteLine ("seekorigin done");
				input.Seek (0, SeekOrigin.Begin);
				//Console.WriteLine ("input seek done");
				return ms.ToArray();
			}
		}

		public Stream BytesToStream(byte[] image){
			//Console.WriteLine ("In BytesToStream");
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

