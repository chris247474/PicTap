using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Contacts;

namespace PicTap
{
	public static class PostImageRecognitionActions
	{
		static string openin = "Export";
		static string saveto = "Save to Contacts";

		public static async Task OpenIn(string firstname, string lastname,
			 CNLabeledValue<CNPhoneNumber>[] numbers, string org) 
		{
			var result = await UserDialogs.Instance.ActionSheetAsync(
				string.Format("What do we do with contact {0} {1}", firstname, lastname), null, null, null, 
			    new string[] { 
					openin,
					saveto
				}
			);

			if (string.Equals(result, openin))
			{
				await NativeDeviceUtil.Share(CombineContactDataForExporting(firstname, lastname, numbers, org));
			}
			else if (string.Equals(result, saveto)){
				ContactsHelper.PushNewContactDialogue(firstname, lastname,
			   		numbers, org);
			}
		}

		static string CNPhoneNumbersToStrings(CNLabeledValue<CNPhoneNumber>[] numbers)
		{
			string result = "Contact Numbers\n";
			for (int c = 0; c < numbers.Length; c++) {
				result += "\t"+ numbers[c].Label+ ": " + numbers[c].Value.StringValue + "\n\n";
			}
			return result;
		}

		static string CombineContactDataForExporting(string fname, string lname, CNLabeledValue<CNPhoneNumber>[] numbers,
		    string org) 
		{
			return string.Format("Name: {0} {1} \n {2} Organization: {3}", fname, lname, CNPhoneNumbersToStrings(numbers), org);
		}
	}
}

