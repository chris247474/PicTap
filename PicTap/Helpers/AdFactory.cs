using System;
using System.Linq;
using CoreGraphics;
using Google.MobileAds;
using UIKit;

namespace PicTap
{
	public class AdFactory
	{
		static UIWindow window;
		static UIViewController navController;

		static BannerView adViewTableView;
		static BannerView adViewWindow;
		static Interstitial adInterstitial;

		static bool adOnTable = false;
		static bool adOnWindow = false;
		static bool interstitialRequested = false;

		const string bannerId = "ca-app-pub-6161089310557130/2498669604";

		public static void AddToWindow(int width = 320)
		{
			window = new UIWindow(UIScreen.MainScreen.Bounds);
			navController = iOSNavigationHelper.GetUINavigationController();

			if (adViewWindow == null)
			{

				// Setup your GADBannerView, review AdSizeCons class for more Ad sizes. 
				adViewWindow = new BannerView(AdSizeCons.SmartBannerPortrait,
					origin: new CGPoint(0, window.Bounds.Size.Height - AdSizeCons.Banner.Size.Height))
				{
					AdUnitID = bannerId,
					RootViewController = UIApplication.SharedApplication.KeyWindow.RootViewController//navController
				};

				// Wire AdReceived event to know when the Ad is ready to be displayed
				adViewWindow.AdReceived += (object sender, EventArgs e) =>
				{
					if (!adOnWindow)
					{
						navController.View.Subviews.First().Frame = new CGRect(0, 0, width, 
						    UIScreen.MainScreen.Bounds.Height - 50);
						navController.View.AddSubview(adViewWindow);
						adOnWindow = true;
					}
				};
			}
			adViewWindow.LoadRequest(Request.GetDefaultRequest());
		}
	}
}

