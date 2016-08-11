using System;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
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

		const string bannerId = //"ca-app-pub-6161089310557130/2498669604";//ConTap banner
			"ca-app-pub-6161089310557130/1672786403";//PicTap banner
		const string intersitialId = //"ca-app-pub-6161089310557130/5823784407";//ConTap
			"ca-app-pub-6161089310557130/3149519606";//PicTap
		const string nativeId = "ca-app-pub-6161089310557130/2032713204";//PicTap

		public static void AdjustViewControllerLayoutForAds(UIViewController vc) {
			if (vc == null) throw new NullReferenceException("ViewController param cannot be null");

			try { 
				vc.SetValueForKey(NSNumber.FromBoolean(true), new NSString("canDisplayBannerAds"));
			} catch (Exception e) {
				Console.WriteLine("AdjustViewControllerLayoutForAds error: {0}", e.Message);
			}
		}

		public static async void ShowInterstitial()
		{
			adInterstitial = new Interstitial(intersitialId);
			adInterstitial.LoadRequest(Request.GetDefaultRequest());

			// We need to wait until the Intersitial is ready to show
			do
			{
				await Task.Delay(100);
			} while (!adInterstitial.IsReady);

			adInterstitial.PresentFromRootViewController(GlobalVariables.VCToInvokeOnMainThread);
			// Once ready, show the ad on Main thread
			/*if (GlobalVariables.VCToInvokeOnMainThread != null)
			{
				Console.WriteLine("vc for ad: {0}", GlobalVariables.VCToInvokeOnMainThread == null);
				GlobalVariables.VCToInvokeOnMainThread.InvokeOnMainThread(() =>
					 adInterstitial.PresentFromRootViewController(navController));
			}
			else Console.WriteLine("VC for ads is null");*/
		}

		public static void ShowBanner(int width = 320)
		{
			Console.WriteLine("in ShowBanner");
			//await Task.Delay(5000);
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

