using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Contacts;
using Foundation;
using UIKit;

namespace PicTap
{
	public static class PostImageRecognitionActions
	{
		static string openin = "Export";
		static string saveto = "Save to Contacts";
		static string copyto = "Copy For Pasting";

		public static async void OpenIn(CNMutableContact contact, string textClipboard = "") 
		{
			var result = await UserDialogs.Instance.ActionSheetAsync(
				string.Format("What do we do with contact {0} {1}", contact.GivenName, contact.FamilyName), null, 
				null, null, 
				(string.IsNullOrWhiteSpace(textClipboard) ? new string[] { saveto, openin } : 
				 new string[] { saveto, openin, copyto})

			);

			if (string.Equals(result, openin))
			{
				NativeDeviceUtil.Share(CombineContactDataForExporting(contact));
			}
			else if (string.Equals(result, saveto))
			{
				ContactsHelper.PushNewContactDialogue(contact);
			}else if (string.Equals(result, copyto))
			{
				ClipBoardService.CopyToClipboard(textClipboard);
				UserDialogs.Instance.Alert("Copied!", null, "OK");
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

		static string CNEmailsToStrings(CNLabeledValue<NSString>[] emails)
		{
			string result = "\n";
			for (int c = 0; c < emails.Length; c++)
			{
				result += "\t" + emails[c].Label + ": " + emails[c].Value + "\n\n";
			}
			return result;
		}

		static string CombineContactDataForExporting(CNMutableContact contact) 
		{
			return string.Format("Name: {0} {1}\n{2}Organization: {3}\nEmail Addresses:{4}", 
			                     contact.GivenName, contact.FamilyName, CNPhoneNumbersToStrings(contact.PhoneNumbers), 
			                     contact.OrganizationName, CNEmailsToStrings(contact.EmailAddresses));
		}
	}
}

