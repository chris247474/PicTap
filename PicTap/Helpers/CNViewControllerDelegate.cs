﻿using System;
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

			try{
			}catch(Exception e){
				ContactsHelper.DismissCNContactViewControllerWithToolBarItemsOutsideUINavigationController(true, null);
				Console.WriteLine ("CNViewControllerDelegate error: {0}", e.Message);
			}
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
