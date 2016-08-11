using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Acr.UserDialogs;

namespace PicTap
{
	/// <summary>
	/// Startup related functions. Usually called in AppDelegate/Main, except for StoreEmail method (sometimes crashes 
	/// if root window is not yet setup/initialized)
	/// </summary>
	public static class UserInteractionHelper
	{
		public const string NEXTTIME = "Next Time";
		public const string DONTREMIND = "Don't ask again";
		public const int DAYSBEFOREASKINGFOREMAIL = 2;

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

		static bool TimeToAskForEmail() {
			var timetoask = (Settings.AskAgainSettings && !Settings.IsFirstRunSettings &&
					DateTime.Today.Date >= Settings.InstallDateSettings.Date.AddDays(
				                 DAYSBEFOREASKINGFOREMAIL) &&
							 Settings.InstallDateSettings.Date > DateTime.MinValue.Date);
			Debug.WriteLine("Time To ask for email: {0}", timetoask);
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

