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

namespace PicTap
{
	public class ImagePreProcessor
	{
		const int SUCCESSTHRESHOLD = 28;
		public const double TIMEOUTLIMIT = 6000;
		CNMutableContact contact;
		CNMutablePostalAddress postalAddress;
		bool nameFound = false, numFound = false, emailFound = false, URLFound = false, //addressFound = false,
				orgFound = false, goToNextIteration = false, streetFound = false, citypostalFound = false,
				cityFound = false, jobFound = false;

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



		public async Task<MatchFormat> ReadAnyNameReturnMatchAndRemainderString(string input)
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


		}

		/*public async Task<int> TesseractExtractText(Stream s, Tesseract.OcrEngineMode engineMode,
										  Tesseract.PageSegmentationMode segMode, string correctOutput, int ctr = 0)
		{
			Console.WriteLine("Begin Tesseract Test: EngineMode {0}, SegmentationMode: {1}",
							  engineMode,
							  segMode);

			string text = string.Empty;
			//TesseractApi api = new TesseractApi();
			int LD = 1000000;

			try
			{

				text = await ExtractTextFromImage_ProjectOxford(s);

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
			return (LD > SUCCESSTHRESHOLD) ? 0 : 1;
		}*/

		/*public async Task<int> ExtractTextTest(Stream s, Tesseract.OcrEngineMode engineMode,
										  Tesseract.PageSegmentationMode segMode, string correctOutput, int ctr)
		{//create auto test functions and data
			Console.WriteLine("INIT TESSERACT -------------------------------------------------------------------------------------");
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
		}*/



		public async Task RunBusinessCardTest(UIImage[] inputImages)
		{
			List<TestData> averageScores = new List<TestData>();

			/*for (int c = 0; c < inputImages.Length; c++) {
				Console.WriteLine("Adding image {0} to test array", c);
				var s = await PreprocessUIImage(inputImages[c]);
				SaveImageToPhotosApp(s, "");
				inputImages[c] = GetUIImageFromStream(s);
				Console.WriteLine("Done adding image {0} to test array", c);
			}*/

			UserDialogs.Instance.ShowLoading();

			//tesseract only - all possible pagesegmentationmodes
			//await TaskHelper.RunTaskThenCancelAfterTimeout(async () => {
			//averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractOnly,
			//										Tesseract.PageSegmentationMode.Auto));
			//});

			/*await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractOnly,
														Tesseract.PageSegmentationMode.AutoOsd));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractOnly,
													Tesseract.PageSegmentationMode.CircleWord));
			});


			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractOnly,
													Tesseract.PageSegmentationMode.SingleBlock));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractOnly,
													Tesseract.PageSegmentationMode.SingleBlockVertText));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractOnly,
													Tesseract.PageSegmentationMode.SingleChar));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractOnly,
													Tesseract.PageSegmentationMode.SingleColumn));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractOnly,
													Tesseract.PageSegmentationMode.SingleLine));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractOnly,
													Tesseract.PageSegmentationMode.SingleWord));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractOnly,
													Tesseract.PageSegmentationMode.SparseText));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractOnly,
													Tesseract.PageSegmentationMode.SparseTextOsd));
			});


			//cubonly - all possible segmentationmodes
			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.CubeOnly,
														Tesseract.PageSegmentationMode.Auto));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.CubeOnly,
														Tesseract.PageSegmentationMode.AutoOsd));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.CubeOnly,
													Tesseract.PageSegmentationMode.CircleWord));
			});


			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.CubeOnly,
													Tesseract.PageSegmentationMode.SingleBlock));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.CubeOnly,
													Tesseract.PageSegmentationMode.SingleBlockVertText));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.CubeOnly,
													Tesseract.PageSegmentationMode.SingleChar));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.CubeOnly,
													Tesseract.PageSegmentationMode.SingleColumn));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.CubeOnly,
													Tesseract.PageSegmentationMode.SingleLine));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.CubeOnly,
													Tesseract.PageSegmentationMode.SingleWord));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.CubeOnly,
													Tesseract.PageSegmentationMode.SparseText));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.CubeOnly,
													Tesseract.PageSegmentationMode.SparseTextOsd));
			});


			//combined - all possible segmentationmodes
			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractCubeCombined,
														Tesseract.PageSegmentationMode.Auto));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractCubeCombined,
														Tesseract.PageSegmentationMode.AutoOsd));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractCubeCombined,
													Tesseract.PageSegmentationMode.CircleWord));
			});


			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractCubeCombined,
													Tesseract.PageSegmentationMode.SingleBlock));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractCubeCombined,
													Tesseract.PageSegmentationMode.SingleBlockVertText));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractCubeCombined,
													Tesseract.PageSegmentationMode.SingleChar));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractCubeCombined,
													Tesseract.PageSegmentationMode.SingleColumn));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractCubeCombined,
													Tesseract.PageSegmentationMode.SingleLine));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractCubeCombined,
													Tesseract.PageSegmentationMode.SingleWord));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractCubeCombined,
													Tesseract.PageSegmentationMode.SparseText));
			});

			await TaskHelper.RunTaskThenCancelAfterTimeout(async () =>
			{
				averageScores.Add(await GetAverageScore(inputImages, Tesseract.OcrEngineMode.TesseractCubeCombined,
													Tesseract.PageSegmentationMode.SparseTextOsd));
			});*/

			Console.WriteLine("Done with all possible combinations of OcrengineModes and PagesegmentationModes");

			foreach (TestData score in averageScores)
			{
				Console.WriteLine("\n\n\n ----------- Average score for config {0}, {1} is {2} -----------", score.engineName, score.segName, score.Average);
			}

			UserDialogs.Instance.HideLoading();
		}

		/*public async Task<TestData> GetAverageScore(UIImage[] inputImages, Tesseract.OcrEngineMode engineMode,
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
					successScores.Add(await ExtractTextTest(image, engineMode, segMode, correctStringOutputs[c], c));
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

			return null;
			//Console.WriteLine("----------- AVERAGE SUCCESS SCORE: TesseractOnly, AutoOnly IS {0} ----------------", average);
		}*/

		/*public void RunTest(int imageCount)
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
		}*/

		public async Task<List<Tesseract.Result>> ExtractTextFromImage_Tesseract(Stream image,
		    Tesseract.PageIteratorLevel recognitionLevel = Tesseract.PageIteratorLevel.Textline)
		{
			List<Tesseract.Result> imageResult = null;
			TesseractApi api = new TesseractApi();
			try
			{
				if (await api.Init("eng"))
					if (await api.SetImage(image))
					imageResult = api.Results(recognitionLevel).ToList();

			}
			catch (Exception e)
			{
				System.Console.WriteLine("[Pics.loadFromPicDivideBy] Error loading picture: " + e.Message);
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

		public void ReadBusinessCardThenSaveExportHandleTimeout(Stream image)
		{
			var cancelTokenSource = new CancellationTokenSource();
			var cancelToken = cancelTokenSource.Token;

			try
			{
				var readTask = Task.Run(() => {
					//ReadBusinessCardThenSaveExport(image, cancelToken, true);
				}, cancelToken);
			//cancelTokenSource.CancelAfter((int)TIMEOUTLIMIT);
			//await Task.Delay((int)TIMEOUTLIMIT+200);
			//Console.WriteLine("Disposing readTask");
			//readTask.Dispose();
			//readTask = null;
			//Console.WriteLine("readTask is null: {0}", readTask == null);
			}
			catch (TimeoutException te)
			{
				Console.WriteLine("ReadBusinessCardThenSaveExport error: {0}", te.Message);
				//UserDialogs.Instance.Alert("Image could not be read", "Oops", "OK");
			}

			
		}

		public async Task ReadBusinessCardThenSaveExport_Tesseract(UIImage image, UIProgressView progressBar, 
		                                                           UIActivityIndicatorView loadingView)
		{
			Console.WriteLine("in ReadBusinessCardThenSaveExport_Tesseract");

			if (image == null) throw new ArgumentNullException("Illegal null value passed to ImagePreprocessor" +
															   ".ReadBusinessCardThenSaveExport(stream)");
			ResetContactReaderData();
			int progressCtr = 0;

			ShowStartLoading(progressBar, loadingView);

			//Preprocess image for better text recognition results - adds too much overhead time
			Stream preProcessedStream = await PreprocessUIImage(image);

			//save for testing purposes
			//SaveImageToPhotosApp(preProcessedStream, System.DateTime.Now.Second + "bwsharp.png");

			var result = await ExtractTextFromImage_Tesseract(preProcessedStream);
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
			PostImageRecognitionActions.OpenIn(contact, wholeTextForClipboard);
		}

		void ShowStartLoading(UIProgressView progressBar, UIActivityIndicatorView loadingView)
		{
			progressBar.Hidden = false;
			loadingView.StartAnimating();
			progressBar.SetProgress(0.25f, true);
		}
			async void ShowDoneLoading(UIProgressView progressBar, UIActivityIndicatorView loadingView) { 
			progressBar.SetProgress(1.0f, true);
			await Task.Delay(1000);
			progressBar.Hidden = true;
			loadingView.StopAnimating();
			progressBar.SetProgress(0.0f, false);
		}

		public async Task ReadBusinessCardThenSaveExport_MicrosoftVision(Stream image, UIProgressView progressBar, 
		                                                 UIActivityIndicatorView loadingView)//CancellationToken cancelToken,
	                                                     //bool canTimeout = false) 
		{
			ShowStartLoading(progressBar, loadingView);
			
			if (image == null) throw new ArgumentNullException("Illegal null value passed to ImagePreprocessor" +
		                                                       ".ReadBusinessCardThenSaveExport(stream)");
			OcrResults result = null;
			//contact = new CNMutableContact();
			//postalAddress = new CNMutablePostalAddress();
			ResetContactReaderData();
			int progressCtr = 0;
			string wholeTextForClipboard = "";

			/*if (canTimeout){
				Task.Run(async () =>
				{
					for (int i = 0; i < TIMEOUTLIMIT / 1000; i++)
					{
						await Task.Delay(1000);
						Console.WriteLine("In Task Timer: {0}", i);// Cancel requested? {1}", i, cancelToken.IsCancellationRequested);
						if (result == null) throw new TimeoutException("Image took too long to read");//cancelToken.ThrowIfCancellationRequested();
						else Console.WriteLine("Text extracted from image");
					}
				});
			}*/
			result = await ExtractOCRResultFromImage_ProjectOxford(image);
			progressBar.SetProgress(0.5f, true);

			foreach (var region in result.Regions)
			{
				Console.WriteLine("In regions");
				foreach (var line in region.Lines)
				{
					Console.WriteLine("In lines");
					wholeTextForClipboard += "\n" + CombineWordsIntoLine(line);
					progressCtr++;
					var readingProgress = progressBar.Progress + (float)(progressCtr / (float)region.Lines.Count());
					progressBar.SetProgress(readingProgress, true);
					goToNextIteration = false;//reset

					//compare line to regex
					Console.WriteLine("about to process textline: nameFound? {0}", nameFound);
					if (!nameFound)
					{
						nameFound = IsName(CombineWordsIntoLine(line));
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
					numFound = IsNumber(CombineWordsIntoLine(line));
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
						orgFound = IsOrg(CombineWordsIntoLine(line));
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
						emailFound = IsEmail(CombineWordsIntoLine(line));
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
						URLFound = IsURL(CombineWordsIntoLine(line));
						if (URLFound)
						{
							goToNextIteration = true;
							Console.WriteLine("Found URL in text line, skipping to next line");
						}
						else goToNextIteration = false;
					}
					Console.WriteLine("Done checking for URL, gotonextiteration {0}", goToNextIteration);
					if (goToNextIteration) continue;

					CheckAddress(CombineWordsIntoLine(line));

					Console.WriteLine("about to process textline: jobFound? {0}", jobFound);
					if (!jobFound)
					{
						jobFound = IsJob(CombineWordsIntoLine(line));
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
			}

			//for some reason, only first call to SaveAddressToContact() actually saves to CNPostalAddress, 
			//so call after loop for now
			SaveAddressToContact();

			ShowDoneLoading(progressBar, loadingView);
			PostImageRecognitionActions.OpenIn(contact, wholeTextForClipboard);
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
			else Console.WriteLine("No single match city found");
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
			else Console.WriteLine("IsCityAndPostal: No single match citypostal found");
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
				//contact.PostalAddresses = new CNLabeledValue<CNPostalAddress>[]{};
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
						Console.WriteLine("found email label: {0}", labelMatch.Groups[0].Value);
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
				if (HasCompanySuffix(nameMatch.Groups[0].Value)) {
					contact.OrganizationName += nameMatch.Groups[0].Value;
					orgFound = true;//?
					return false;
				}
				if (IsJob(nameMatch.Groups[0].Value)) return false;

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
		bool HasCompanySuffix(string input) { 
			var companyStrings = RegexHelper.COMPANYSTRINGS.Split(new char[] { ' ', '\t' });
			Console.WriteLine("{0} company strings in memory", companyStrings.Length);
			for (int c = 0; c < companyStrings.Length; c++)
			{
				Console.WriteLine("string {0} is {1}", c, companyStrings[c]);
				if (input.ToLower().Contains(companyStrings[c].ToLower()))
				{
					Console.WriteLine("Found company match: {0}", companyStrings[c]);
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

				//Console.WriteLine("Number match is {0}", numMatch.Groups[0].Value);
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
		}*/
		void IfObjectNullExecuteAction(object match, Action recognitionAction) {
			if (match == null) recognitionAction();
		}
		/*public async Task ReadImageTextThenSaveToPhoneContacts(Stream bwSharpenedStream, bool singledetect = true,
			UIView View = null, bool secondattempt = false)
		{
			Console.WriteLine("In ReadImageTextThenSaveToDB");
			//var progressBar = iOSUIBuilder.ShowCircularProgressBar(View, "Reading Image");
			UserDialogs.Instance.ShowLoading("Reading Image", MaskType.Gradient);

			string error = null;
			string tempError = null;
			string firstname, lastname, number, aff;
			int errorStatus;
			List<CNMutableContact> saveList = new List<CNMutableContact>();
			Tesseract.Result result;
			//CNMutableContact ContactToSave;

			if (bwSharpenedStream == null)
			{
				throw new ArgumentNullException("ReadImageTextThenSaveToDB bwSharpenedStream param is null");
			}

			var imageResult = await ExtractTextFromImage(bwSharpenedStream);//imageList.ToArray();

			if (imageResult == null)
			{
				UserDialogs.Instance.Alert("Unable to load contacts", "{0} couldn't read the image", "OK");
			}
			else {

				for (int c = 0; c < (singledetect ? 1 : imageResult.Length);c++)//foreach (Tesseract.Result result in imageResult)
				{
					errorStatus = 0;
					firstname = null;
					lastname = null;
					number = null;
					aff = null;
					result = imageResult[c];

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
						Console.WriteLine("MINUS SPECIAL CHARS: " + RegexHelper.RemoveCountryCodeReadingErrorsAndSpecialChar(result.Text, new Regex(Values.COUNTRYCODE), specialReg));

						Match nameNumOrgNotesMatch = nameNumOrgNotesRegex.Match(RegexHelper.RemoveCountryCodeReadingErrorsAndSpecialChar(result.Text, new Regex(Values.COUNTRYCODE), specialReg));
						Match nameNumMatch = nameNumRegex.Match(RegexHelper.RemoveCountryCodeReadingErrorsAndSpecialChar(result.Text, new Regex(Values.COUNTRYCODE), specialReg));
						Match wordMatch = wordReg.Match(RegexHelper.RemoveCountryCodeReadingErrorsAndSpecialChar(result.Text, new Regex(Values.COUNTRYCODE), specialReg));

						if (nameNumOrgNotesMatch.Success )
						{//check for different types of name and number combinations?
							//progressBar.SetProgress(50, true);

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
							//progressBar.SetProgress(75, true);

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

						Console.WriteLine("ERROR SAVING CONTACT - SKIPPING: " + error);
					}
				}
			}
			UserDialogs.Instance.HideLoading();
			//progressBar.SetProgress(100, true);
			//progressBar.RemoveFromSuperview();

			//refractor to save all contacts
			var singlecontacttest = saveList.ElementAt(0);
			ContactsHelper.PushNewContactDialogue(singlecontacttest.GivenName, singlecontacttest.FamilyName, 
												  singlecontacttest.PhoneNumbers, singlecontacttest.OrganizationName);
			

			PostImageRecognitionActions.OpenIn(singlecontacttest);
		}*/

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
				//Console.WriteLine("Saved Image to photos app");
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
			Console.WriteLine("in PreprocessImage");

			if (transformedcroppedimage != null) {
				//UserDialogs.Instance.ShowLoading ("Cleaning Image...");

				var sharpenedImage = UnSharpMask(transformedcroppedimage);

				var adaptiveThreshImage = AdaptiveThreshold(sharpenedImage);//test

				UIImage finalImage = (adaptiveThreshImage == null) ? sharpenedImage : adaptiveThreshImage;

				var result = GetStreamFromUIImage(finalImage);

				//var result = GetStreamFromUIImage(ApplyGreyScale(transformedcroppedimage));
				
				return result;
			}
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
				Image = imageToSharpen, Radius = 7.0f
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

