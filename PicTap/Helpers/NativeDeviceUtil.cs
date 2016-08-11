using System;
using AddressBook;
using Foundation;
using Acr.UserDialogs;
using UIKit;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace PicTap
{
    class NativeDeviceUtil
	{
		ABAddressBook abb = new ABAddressBook();
		ABPerson[] contacts = null;

		public NativeDeviceUtil(){
			Console.WriteLine ("Storing all contacts in iOS memory");
			contacts = abb.GetPeople ();
		}

		public static async Task Share (string message)
		{
			var messagecontent = message;
			var msg = UIActivity.FromObject (messagecontent);

			var item = NSObject.FromObject (msg);
			var activityItems = new[] { item }; 
			var activityController = new UIActivityViewController (activityItems, null);

			var topController = UIApplication.SharedApplication.KeyWindow.RootViewController;

			while (topController.PresentedViewController != null) {
				topController = topController.PresentedViewController;
			}

			topController.PresentViewController (activityController, true, () => {
			});

		}

		public async Task SendSMS (string number){
			var smsTo = NSUrl.FromString("sms:"+number);
			UIApplication.SharedApplication.OpenUrl(smsTo);
		}

		/*string SaveDefaultImage(ContactData contact){
			string filename = System.IO.Path.Combine (Environment.GetFolderPath 
				(Environment.SpecialFolder.Personal), 
				"placeholder-contact-male.png");

			Console.WriteLine("Assigned default image to {0} {1}. Saving it as {1}", 
				contact.FirstName, contact.LastName, filename);
			
			return filename;
		}
		string SaveImageThenGetPath(ContactData contact, NSData image, ABPersonImageFormat format){
			string filename = "";

			try{
				if(format == ABPersonImageFormat.Thumbnail){
					filename = System.IO.Path.Combine (Environment.GetFolderPath
						(Environment.SpecialFolder.Personal), 
						string.Format("{0}.jpg", contact.ID)); 
				}else{
					filename = System.IO.Path.Combine (Environment.GetFolderPath
						(Environment.SpecialFolder.Personal), 
						string.Format("{0}-large.jpg", contact.ID));
				}

				image.Save (filename, true);

				Console.WriteLine("Found {0} {1}'s image. Saving it as {2}", 
					contact.FirstName, contact.LastName, filename);
				
				return filename;
			}catch(Exception e){
				Console.WriteLine ("Error in SaveImageThenGetPath(): {0}", e.Message);
			}
			return string.Empty;
		}*/
        public bool SaveContactToDevice(string firstName, string lastName, string phone, string aff)
        {
            try {
                ABAddressBook ab = new ABAddressBook();
                ABPerson p = new ABPerson();

                p.FirstName = firstName;
                p.LastName = lastName;
                p.Organization = aff;
				//p.GetImage(ABPersonImageFormat.Thumbnail).

                ABMutableMultiValue<string> phones = new ABMutableStringMultiValue();
                phones.Add(phone, ABPersonPhoneLabel.Mobile);

                p.SetPhones(phones);

                ab.Add(p);
                ab.Save();

				UserDialogs.Instance.ShowSuccess("Contact saved: " + firstName + " " + lastName, 2000);

                return true;
            } catch (Exception e) {
                System.Console.WriteLine("[iOS.PhoneContacts] Couldn't save contact: {0} {1}, {2}", firstName, lastName, e.Message);
				UserDialogs.Instance.ShowError("Failed to save contact: "+ firstName + " " + lastName + ". Pls try again.", 2000);
			}
            return false;
        }
		public byte[] ToByte (NSData data)
		{
			byte[] result = new byte[data.Length];
			Marshal.Copy (data.Bytes, result, 0, (int) data.Length);
			return result;
		}
		public string ToBase64String (NSData data)
		{
			return Convert.ToBase64String (ToByte (data));
		}
    }
}

