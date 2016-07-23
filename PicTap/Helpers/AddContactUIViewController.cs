using System;
using UIKit;
using Contacts;
using ContactsUI;
using Foundation;

namespace PicTap
{
	public class AddContactUIViewController:CNContactViewController
	{
		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);
		}
		public override void ViewDidLoad ()
		{
			Console.WriteLine ("AddContactUIViewController Viewdidload");
			base.ViewDidLoad ();

			// Create a new Mutable Contact (read/write)
			// and attach it to the editor
			NSError error;
			var store = new CNContactStore ();
			var fetchKeys = new [] {CNContactViewController.DescriptorForRequiredKeys};
			var mutableContact = new CNMutableContact ();
			CNContactViewController editor; 

			editor = CNContactViewController.FromNewContact(mutableContact);

			Console.WriteLine ("configuring CNContactViewController");
			// Configure editor
			editor.ContactStore = store;
			editor.AllowsActions = true;
			editor.AllowsEditing = true;
			editor.Delegate = new CNViewControllerDelegate ();
			Console.WriteLine ("done configuring CNContactViewController");

			// Display picker
			var navcontrol = iOSNavigationHelper.GetUINavigationController(
					UIApplication.SharedApplication.KeyWindow.RootViewController) as UINavigationController;

			//Console.WriteLine("navcontrol is {0}", navcontrol.GetType());
			navcontrol.PushViewController(editor, true);

			Console.WriteLine ("Done w function");
		}

		/*CNMutableContact GetMatchingPerson(CNContact[] contacts) {
			CNMutableContact mutableContact;
			for (int c = 0; c < contacts.Length; c++)
			{
				Console.WriteLine("Checking if {0} {1} is who we're looking for",
								  contacts[c].GivenName, contacts[c].FamilyName);
				if (string.Equals(contacts[c].GivenName, App.CurrentContact.FirstName) &&
					string.Equals(contacts[c].FamilyName, App.CurrentContact.LastName) &&
					string.Equals(contacts[c].OrganizationName, App.CurrentContact.Aff))
				{
					mutableContact = (contacts[c].MutableCopy() as CNMutableContact);
					Console.WriteLine("Contact: {0} {1} Aff: {2}", mutableContact.GivenName, mutableContact.FamilyName
				                      , mutableContact.PhoneNumbers[0].Value.StringValue);
					return mutableContact;
				}
			}
			return null;
		}

		bool NumberExistsIn(string[] numbers, CNLabeledValue<CNPhoneNumber>[] phones) {
			for (int ctr = 0; ctr < numbers.Length; ctr++) { 
				for (int c = 0; c < phones.Length; c++)
				{
					var filteredNumber = PhoneUtil.ToNumber_Custom(numbers[ctr]);
					var cnNumber = PhoneUtil.ToNumber_Custom(phones[c].Value.StringValue);
					Console.WriteLine("comparing {0} and {1}", filteredNumber, cnNumber);
					if (string.Equals(filteredNumber, cnNumber))
					{
						Console.WriteLine("{0} and {1} are the same", filteredNumber, cnNumber);
						return true;
					}
				}
			}
			Console.WriteLine("NumberExistsIn returning false");
			return false;
		}

		bool ShouldEditContact(){
			return (App.CurrentContact == null) ? false : true;
		}*/

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			Console.WriteLine ("AddContactUIViewController viewdidappear");
		}
	}
}
