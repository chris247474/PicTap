using System;
using System.Threading.Tasks;
using Contacts;
using Microsoft.ProjectOxford.Vision.Contract;
using UIKit;

namespace PicTap
{
	public static class ContactsRecognitionHelper
	{
		/*public async Task ReadBusinessCardThenSaveExport(Stream image, UIProgressView progressBar,
														 UIActivityIndicatorView loadingView)//CancellationToken cancelToken,
																							 //bool canTimeout = false) 
		{
			progressBar.Hidden = false;
			loadingView.StartAnimating();

			if (image == null) throw new ArgumentNullException("Illegal null value passed to ImagePreprocessor" +
															   ".ReadBusinessCardThenSaveExport(stream)");
			OcrResults result = null;
			contact = new CNMutableContact();
			postalAddress = new CNMutablePostalAddress();
			int progressCtr = 0;
			progressBar.SetProgress(0.0f, false);
			progressBar.SetProgress(0.5f, true);

			result = await ExtractOCRResultFromImage_ProjectOxford(image);

			//Console.WriteLine("Text extracted from image");
			bool nameFound = false, numFound = false, emailFound = false, URLFound = false, addressFound = false,
				orgFound = false, goToNextIteration = false, streetFound = false, citypostalFound = false,
				cityFound = false, jobFound = false;

			foreach (var region in result.Regions)
			{
				Console.WriteLine("In regions");
				foreach (var line in region.Lines)
				{
					Console.WriteLine("In lines");
					progressCtr++;
					var readingProgress = progressBar.Progress + ((float)progressCtr / (float)region.Lines.Count());
					progressBar.SetProgress(readingProgress, true);
					goToNextIteration = false;//reset

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

					Console.WriteLine("about to process textline for address: addressFound? {0}", addressFound);
					if (!addressFound)
					{
						addressFound = IsAddress(CombineWordsIntoLine(line));
						if (addressFound)
						{
							goToNextIteration = true;
							Console.WriteLine("Found address in text line, skipping to next line");
						}
						else goToNextIteration = false;
					}
					Console.WriteLine("Done checking for address, gotonextiteration {0}", goToNextIteration);
					if (goToNextIteration) continue;

				}
			}

			progressBar.SetProgress(1.0f, true);
			await Task.Delay(1000);
			progressBar.Hidden = true;
			loadingView.StopAnimating();

			await PostImageRecognitionActions.OpenIn(contact);
		}*/
	}
}

