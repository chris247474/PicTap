using System;
using ContactsUI;
using Contacts;
using Acr.UserDialogs;
using UIKit;

namespace PicTap
{
	public class CNViewControllerDelegate: CNContactViewControllerDelegate
	{
		public override void DidComplete (CNContactViewController viewController, Contacts.CNContact contact)
		{
			Console.WriteLine ("In DidComplete");
			ContactsHelper.DismissCNContactViewControllerWithToolBarItemsOutsideUINavigationController(true, null);

			if (!Settings.IsPremiumSettings)
			{
				Console.WriteLine("Not premium, showing interstitial");
				AdFactory.ShowInterstitial();
			}

			/*GlobalVariables.VCToInvokeOnMainThread.InvokeOnMainThread(() =>
			{
				var navControl = (iOSNavigationHelper.GetUINavigationController() as UINavigationController);
				navControl.SetNavigationBarHidden(true, true);
			});*/
		}
		bool FieldsFilled(CNContact contact){
			if (!string.IsNullOrWhiteSpace (contact.GivenName) && !string.IsNullOrWhiteSpace (contact.FamilyName) &&
			    contact.PhoneNumbers.Length > 0) {
				return true;
			}
			return false;
		}
	}
}

