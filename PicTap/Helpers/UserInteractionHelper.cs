using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Acr.UserDialogs;

namespace PicTap
{
	/// <summary>
	/// Startup/User account related functions. Usually called in AppDelegate/Main, except for StoreEmail method (sometimes crashes 
	/// if root window is not yet setup/initialized)
	/// </summary>
	public static class UserInteractionHelper
	{
		public const string NEXTTIME = "Next Time";
		public const string DONTREMIND = "Don't ask again";
		public const int DAYSBEFOREASKINGFOREMAIL = 2;
		public const int DAYSBEFOREASKINGFORRATING = 5;
		public const int DAYSBEFOREASKINGFORPREMIUM = 7;

		public static void SetInstallDateIfFirstRun()
		{
			if (Settings.InstallDateSettings == DateTime.MinValue)
			{
				Settings.InstallDateSettings = DateTime.Today.Date;
				Debug.WriteLine("Install Date: {0}", Settings.InstallDateSettings);
			}
			else {
				Debug.WriteLine("Install Date already set");
			}
		}
		public static void SetAsPremium(bool isPremium)
		{
			//if not premium, ads will be shown
			Settings.IsPremiumSettings = isPremium;
		}

		public static async void OfferPremium() {
			if (TimeToOfferPremium()) { 
				var destroyAds = await UserDialogs.Instance.ConfirmAsync(
					"Don't want ads?",
					"Destroy those ads!",
				   "Eradicate those ads!!", "I like ads");

				if (destroyAds) { 
					
				} else {
					
				}
			}
		}

		public static async Task AskForRating() {
			if (TimeToAskForRating()) { 
				var itsgreat = await UserDialogs.Instance.ConfirmAsync(
					string.Format("Is {0} working for you?", Values.APPNAME),
					string.Format("Liking {0} so far?", Values.APPNAME),
				   "Yes!!", "There's some issues");
				if (itsgreat)
				{
					var willReview = await UserDialogs.Instance.ConfirmAsync(
						"Quick review?",
						"Great!",
						  "OK", "Later on");

					if (willReview)
					{
						//link to app store, then mark Settings.UserRatedApp true
						throw new NotImplementedException("No link to app store impl yet");
					}
				}
				else {
					throw new NotImplementedException("No recipient in feedback email yet");
					EmailService.SendEmail("",
					   string.Format("Hi! \n\n I've been running into the following issues with {0}: " +
					   "\n\t * Please type any issues here and we'll be sure to do what we can to fix it for you! :) *",
									 Values.APPNAME),
					   "User feedback");
				}
			}
		}

		static bool TimeToOfferPremium()
		{
			var timetoask = (!Settings.IsPremiumSettings && !Settings.IsFirstRunSettings &&
					DateTime.Today.Date >= Settings.InstallDateSettings.Date.AddDays(
								 DAYSBEFOREASKINGFORPREMIUM) &&
							 Settings.InstallDateSettings.Date > DateTime.MinValue.Date);
			Debug.WriteLine("Time to offer premium: {0}", timetoask);
			return timetoask;
		}
		static bool TimeToAskForEmail() {
			var timetoask = (Settings.AskAgainSettings && !Settings.IsFirstRunSettings &&
					DateTime.Today.Date >= Settings.InstallDateSettings.Date.AddDays(
				                 DAYSBEFOREASKINGFOREMAIL) &&
							 Settings.InstallDateSettings.Date > DateTime.MinValue.Date);
			Debug.WriteLine("Time To ask for email: {0}", timetoask);
			return timetoask;
		}
		static bool TimeToAskForRating()
		{
			var timetoask = (!Settings.UserRatedApp && !Settings.IsFirstRunSettings &&
					DateTime.Today.Date >= Settings.InstallDateSettings.Date.AddDays(
				                 DAYSBEFOREASKINGFORRATING) &&
							 Settings.InstallDateSettings.Date > DateTime.MinValue.Date);
			Debug.WriteLine("Time To ask for rating: {0}", timetoask);
			return timetoask;
		}

		public static void CheckIfPremiumShowAdsIfNot() {
			//show ads if not on premium
			if (!Settings.IsPremiumSettings){
				AdFactory.ShowBanner();
			}
			Console.WriteLine("Premium user: {0}", Settings.IsPremiumSettings);
		}

		public static void MarkIfFirstRun() {
			//if first run, then mark first run
			if (Settings.IsFirstRunSettings) Settings.IsFirstRunSettings = false;
		}

		public static async Task StoreUserEmail()
		{
			Debug.WriteLine("AskAgain: {0}, FirstRun: {1}", Settings.AskAgainSettings, Settings.IsFirstRunSettings);
			if (TimeToAskForEmail())
			{
				var itsgreat = await UserDialogs.Instance.ConfirmAsync(
																   "Speed up your productivity even more?",
																	string.Format("Liking {0} so far?", Values.APPNAME),
																   "Take a look", "not now");
				if (itsgreat)
				{
					var emailResult = await UserDialogs.Instance.PromptAsync("Let's keep in touch!", "Great!", "OK", null,
																							 "Please enter your email",
																							  InputType.Email);
					var email = emailResult.Text;
					if (string.IsNullOrWhiteSpace(email))
					{
						email = (await UserDialogs.Instance.PromptAsync("Sorry, didn't seem to get that", "Blank text", "OK", null
																		, "Please enter your email", InputType.Email)).Text;

						if (string.IsNullOrWhiteSpace(email))
						{
							Settings.AskAgainSettings = true;
						}
					}
					else {
						Settings.EmailSettings = email;
						Settings.AskAgainSettings = false;
					}
				}
				else {
					var askagain = await UserDialogs.Instance.ActionSheetAsync("Next time?", null, null, null, new string[] {
						"OK",
						DONTREMIND});
					Settings.AskAgainSettings = string.Equals(askagain, NEXTTIME) ? true : false;
				}
			}
		}
	}
}

