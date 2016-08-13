using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;

namespace PicTap
{
  /// <summary>
  /// This is the Settings static class that can be used in your Core solution or in any
  /// of your client applications. All settings are laid out the same exact way with getters
  /// and setters. 
  /// </summary>
  public static class Settings
  {
    private static ISettings AppSettings
    {
      get
      {
        return CrossSettings.Current;
      }
    }

	#region IsPremiumSettings
	private const string IsPremiumKey = "ispremium";
	private static bool IsPremiumDefault = false;
	#endregion

	public static bool IsPremiumSettings
	{
		get
		{
			return AppSettings.GetValueOrDefault<bool>(IsPremiumKey, IsPremiumDefault);
		}
		set
		{
			AppSettings.AddOrUpdateValue<bool>(IsPremiumKey, value);
		}
	}

	#region AskAgain
	private const string AskAgainKey = "askagain";
	private static bool AskAgainDefault = true;
	#endregion

	public static bool AskAgainSettings
	{
		get
		{
			return AppSettings.GetValueOrDefault<bool>(AskAgainKey, AskAgainDefault);
		}
		set
		{
			AppSettings.AddOrUpdateValue<bool>(AskAgainKey, value);
		}
	}

	#region AskRating
	private const string AskRatingKey = "askrating";
	private static bool AskRatingDefault = false;
	#endregion

	public static bool UserRatedApp
	{
		get
		{
			return AppSettings.GetValueOrDefault<bool>(AskRatingKey, AskRatingDefault);
		}
		set
		{
			AppSettings.AddOrUpdateValue<bool>(AskRatingKey, value);
		}
	}

	#region IsFirstRun
	private const string IsFirstRunKey = "IsFirstRun";
	private static bool IsFirstRunDefault = true;
	#endregion

	public static bool IsFirstRunSettings
	{
		get
		{
			return AppSettings.GetValueOrDefault<bool>(IsFirstRunKey, IsFirstRunDefault);
		}
		set
		{
			AppSettings.AddOrUpdateValue<bool>(IsFirstRunKey, value);
		}
	}
	
	#region TutorialSettings
	private const string TutorialKey = "tutorialshown";
	private static bool DefaultTutorialShown = false;
	#endregion

	public static bool TutorialShownSettings
	{
		get
		{
			return AppSettings.GetValueOrDefault<bool>(TutorialKey, DefaultTutorialShown);
		}
		set
		{
			AppSettings.AddOrUpdateValue<bool>(TutorialKey, value);
		}
	}

	#region InstallDateSettings
	private const string InstallDate = "installdatekey";
	private static readonly DateTime InstallDateDefault = DateTime.MinValue;
	#endregion

	public static DateTime InstallDateSettings
	{
		get
		{
			return AppSettings.GetValueOrDefault<DateTime>(InstallDate, InstallDateDefault);
		}
		set
		{
			AppSettings.AddOrUpdateValue<DateTime>(InstallDate, value);
		}
	}

	

	#region Setting Email Constants
	private const string EmailKey = "email_key";
	private static readonly string EmailDefault = "";
	#endregion

	public static string EmailSettings
	{
		get
		{
			return AppSettings.GetValueOrDefault<string>(EmailKey, EmailDefault);
		}
		set
		{
			AppSettings.AddOrUpdateValue<string>(EmailKey, value);
		}
	}


	#region Setting CountKey Constants
	const string CountKey = "count"; 
	private static readonly int CountDefault = 0; 
	#endregion

	public static int Count { 
		get { return AppSettings.GetValueOrDefault<int>(CountKey, CountDefault); } 
		set { AppSettings.AddOrUpdateValue<int>(CountKey, value); } 
	}

	
	
  }
}