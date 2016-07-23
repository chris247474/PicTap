using System;
using System.Linq;
using Foundation;
using UIKit;

namespace PicTap
{
	public static class iOSNavigationHelper
	{
		public static void PushCNContactViewControllerWithToolBarItemsOutsideUINavigationController(
			UIViewController controlToPush)
		{
			var navcontrol = new UINavigationController();
			navcontrol.PushViewController(controlToPush, true);
			var vc = UIApplication.SharedApplication.KeyWindow.RootViewController;
			while (vc.PresentedViewController != null)
			{
				vc = vc.PresentedViewController;
			}
			vc.PresentViewController(navcontrol, true, () => { });
		}

		public static void DismissCNContactViewControllerWithToolBarItemsOutsideUINavigationController(
			bool animated, Action completionHandler = null)
		{
			iOSNavigationHelper.GetUINavigationController().DismissViewController(animated, completionHandler);
		}

		public static bool IsUINavigationViewController(NSObject view)
		{
			if (view.GetType() == new UINavigationController().GetType())
				return true;
			//if (view.GetType().IsSubclassOf(new UINavigationController().GetType()))
			//	return true;
			return false;
		}

		public static UIViewController GetUINavigationController() {
			return GetUINavigationController(UIApplication.SharedApplication.KeyWindow.RootViewController);
		}

		public static UIViewController GetUINavigationController(UIViewController controller)
		{
			if (controller != null)
			{
				Console.WriteLine("controller is not null");
				if (IsUINavigationViewController(controller))
				{
					Console.WriteLine("Found uinavigationcontroller");
					return (controller as UINavigationController);
				}

				if (controller.ChildViewControllers.Count() != 0)
				{
					var count = controller.ChildViewControllers.Count();

					for (int c = 0; c < count; c++)
					{
						Console.WriteLine(
							"local iteration {0}: current controller has {1} children", c, count);
						var child = GetUINavigationController(controller.ChildViewControllers[c]);
						if (child == null)
						{
							Console.WriteLine("No children left on current controller. Moving back up");
						}
						else if (IsUINavigationViewController(child))
						{
							Console.WriteLine("returning customnavigationrenderer");
							return (child as UINavigationController);
						}
					}
				}
			}

			Console.WriteLine("no UINavigationController found");

			return GetTopUIViewController();
		}

		public static UIViewController GetTopUIViewController() { 
			var vc = UIApplication.SharedApplication.KeyWindow.RootViewController;
			while (vc.PresentedViewController != null)
			{
				vc = vc.PresentedViewController;

			}
			Console.WriteLine("Returning vc of type: {0}", vc.GetType());
			return vc;
		}
	}
}

