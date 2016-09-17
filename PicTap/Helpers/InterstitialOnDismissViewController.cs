using System;
using UIKit;

namespace PicTap
{
	public class InterstitialOnDismissViewController:UIViewController
	{
		public override void DismissViewController(bool animated, Action completionHandler)
		{
			base.DismissViewController(animated, completionHandler);

			if (!Settings.IsPremiumSettings)
			{
				Console.WriteLine("InterstitialOnDismissViewController: Not premium, showing interstitial");
				AdFactory.ShowInterstitial();
			}
		}
	}
}

