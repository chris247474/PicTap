using Foundation;
using System;
using UIKit;
using IPDFCamera_Binding;
using CoreGraphics;
using CoreAnimation;
using System.Threading.Tasks;
using Contacts;

namespace PicTap
{
    public partial class IPDFViewController : UIViewController
    {
		ImagePreProcessor ImageHelper = new ImagePreProcessor();
		UIImageView captureImageView = new UIImageView();

		WeakReference weakSelf;
		IPDFViewController WeakSelf
		{
			get
			{
				if (weakSelf == null || !weakSelf.IsAlive)
					return null;
				return weakSelf.Target as IPDFViewController;
			}
		}

		public IPDFViewController() : base ("IPDFViewController", null)
		{
			weakSelf = new WeakReference(this);
			if (weakSelf == null) Console.WriteLine("weakSelf is null"); else Console.WriteLine("not null");
		}

        public IPDFViewController (IntPtr handle) : base (handle)
        {
        }

		public override async void ViewDidLoad()
		{
			base.ViewDidLoad();

			/*	separate simulator testing

			await Task.Delay(2000);
			await PostImageRecognitionActions.OpenIn("firstname", "secondname",
												  new CNLabeledValue<CNPhoneNumber>[] {
				new CNLabeledValue<CNPhoneNumber>("mobile", new CNPhoneNumber("09163247357"))}, "World");
			/
			await Task.Delay(2000);
			AdFactory.AddToWindow();*/

			weakSelf = new WeakReference(this);
			if (weakSelf == null) Console.WriteLine("weakSelf is null"); else Console.WriteLine("not null");

			ipdfView.SetupCameraView();
			ipdfView.EnableBorderDetection = true;
		}

		public override void ViewDidAppear(bool animated)
		{
			Console.WriteLine("ViewDidAppear");
			base.ViewDidAppear(animated);

			UITapGestureRecognizer focusTap = new UITapGestureRecognizer(focusGesture);
			ipdfView.AddGestureRecognizer(focusTap);

			ipdfView.Start();
		}

		void focusGesture(UITapGestureRecognizer sender)
		{
			Console.WriteLine("in focusGesture");
			if (sender.State == UIGestureRecognizerState.Recognized)
			{
				Console.WriteLine("recognized");
				CGPoint location = sender.LocationInView(this.ipdfView);

				this.focusIndicatorAnimateToPoint(location);

				this.ipdfView.FocusAtPoint(location, () =>
				{
					this.focusIndicatorAnimateToPoint(location);
				});
			}
		}

		void updateTitleLabel()
		{
			CATransition animation = new CATransition();
			animation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseInEaseOut);
			animation.Type = CAAnimation.TransitionPush;
			animation.Subtype = CAAnimation.TransitionFromBottom;
			animation.Duration = 0.35;
			this.titleLabel.Layer.AddAnimation(animation, CAAnimation.TransitionFade);

			string filterMode =
				(this.ipdfView.CameraViewType == IPDFCameraViewType.BlackAndWhite) ? "TEXT FILTER" : "COLOR FILTER";
			/*if (this.ipdfView.EnableBorderDetection) {
				this.titleLabel.Text = (NSString)@"AUTOCROP On";
			} else {
				this.titleLabel.Text = (NSString)@"AUTOCROP Off";
			}*/
			this.titleLabel.Text = filterMode + " | " +
				((this.ipdfView.EnableBorderDetection) ? "AUTOCROP On" : "AUTOCROP Off");
		}

		void changeButton(UIButton button, NSString title, bool enabled)
		{
			button.SetTitle(title, UIControlState.Normal);
			if (enabled)
			{
				button.SetTitleColor(UIColor.FromRGB((nfloat)1, (nfloat)0.81, (nfloat)0).ColorWithAlpha((nfloat)1),
					UIControlState.Normal);
			}
			else {
				button.SetTitleColor(UIColor.White, UIControlState.Normal);
			}
		}

		void dismissPreview(UITapGestureRecognizer dismissTap)
		{
			UIView.Animate(0.7, 0, UIViewAnimationOptions.AllowUserInteraction,
				() =>
				{
					dismissTap.View.Frame = CGRect.Inflate(this.View.Bounds, 0, this.View.Bounds.Size.Height);
				},
				null
			);
		}

		public async Task SaveImage()
		{
			var filename = System.DateTime.Now.Second + "cropped.png";
			Console.WriteLine("Saving Image, {0}", filename);
			ImageHelper.SaveImageToPhotosApp/*SaveImageToDiskThenNotifyViewModelToStartPreprocessingImage*/(ImageHelper.GetStreamFromUIImage(captureImageView.Image),
				filename);
			Console.WriteLine("Done with IPDFCamera.SaveImage()");
		}

		void focusIndicatorAnimateToPoint(CGPoint targetPoint)
		{
			this.focusIndicator.Center = targetPoint;
			this.focusIndicator.Alpha = (nfloat)0.0;
			this.focusIndicator.Hidden = false;

			UIView.Animate(0.4,
				() =>
				{
					this.focusIndicator.Alpha = (nfloat)0.5;
				},
				() =>
				{
					UIView.Animate(0.4, () =>
					{
						this.focusIndicator.Alpha = (nfloat)0.0;
					});
				}
			);
		}

		partial void CaptureButton_TouchUpInside(UIButton sender)
		{
			this.ipdfView.CaptureImageWithCompletionHander(new Action<NSString>((NSString imageFilePath) =>
			{
				captureImageView = new UIImageView(new UIImage(imageFilePath));
				captureImageView.BackgroundColor = UIColor.White;
				captureImageView.BackgroundColor.ColorWithAlpha((nfloat)0.7);
				captureImageView.Frame = CGRect.Inflate(WeakSelf.View.Bounds, 0, -WeakSelf.View.Bounds.Size.Height);
				captureImageView.Alpha = (nfloat)1.0;
				captureImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
				captureImageView.UserInteractionEnabled = true;
				WeakSelf.View.AddSubview(captureImageView);

				UITapGestureRecognizer dismissTap = new UITapGestureRecognizer(dismissPreview);
				captureImageView.AddGestureRecognizer(dismissTap);

				UIView.Animate(0.7, 0, UIViewAnimationOptions.AllowUserInteraction,
					() =>
					{
						captureImageView.Frame = WeakSelf.View.Bounds;
					},
					async () =>
					{
						await ImageHelper.loadContactsFromPic(captureImageView.Image, true);
					}
				);
			}));
		}

		partial void FilterButton_TouchUpInside(UIButton sender)
		{
			this.ipdfView.CameraViewType = (this.ipdfView.CameraViewType == IPDFCameraViewType.BlackAndWhite) ?
				IPDFCameraViewType.Normal : IPDFCameraViewType.BlackAndWhite;
			this.updateTitleLabel();
		}

		partial void FlashButton_TouchUpInside(UIButton sender)
		{
			try
			{
				bool enable = !this.ipdfView.EnableTorch;
				NSString text = (enable) ? (NSString)@"FLASH On" : (NSString)@"FLASH Off";
				this.changeButton(sender as UIButton, text, enable);
				this.ipdfView.EnableTorch = enable;
			}
			catch (Exception e)
			{
				Console.WriteLine("IPDFCameraViewController.ipdfView.torchToggle error: {0}", e.Message);
			}
		}
	}
}