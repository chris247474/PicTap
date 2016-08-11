using System;
using System.Collections.Generic;
using CoreGraphics;
using DACircularProgress;
using UIKit;

namespace PicTap
{
	public static class iOSUIBuilder
	{
		public static LabeledCircularProgressView ShowCircularProgressBar(UIView View, string title) { 
			var rect = new CGRect(140.0f, 30.0f, 40.0f, 40.0f);

			// create the control
			var progressView = new LabeledCircularProgressView(rect);
			progressView.RoundedCorners = true;
			progressView.TrackTintColor = UIColor.Clear;
			progressView.ClockwiseProgress = true;
			progressView.ProgressLabel.Text = title;

			View.AddSubview(progressView);

			return progressView;
		}

		public static UIScrollView CreateHorizontalScrollButtonView(List<UIButton> buttons, nfloat w, nfloat h, 
		    nfloat padding, nint n, UIView View) 
		{ 

			var scrollView = new UIScrollView
			{
				Frame = new CGRect(0, 100, View.Frame.Width, h + 2 * padding),
				ContentSize = new CGSize((w + padding) * n, h),
				BackgroundColor = UIColor.Clear,
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth
			};

			int size = buttons.Count;
			for (int i = 0; i < size; i++)
			{
				if (buttons[i] != null) { 
					var button = buttons[i];
					button.SetTitle(i.ToString(), UIControlState.Normal);
					button.Frame = new CGRect(padding * (i + 1) + (i * w), padding, w, h);
					//button.TouchUpInside += 
					scrollView.AddSubview(button);
					buttons.Add(button);
				}
			}

			return scrollView;
		}
	}
}

