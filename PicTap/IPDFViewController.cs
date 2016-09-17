using Foundation;
using System;
using UIKit;
using CoreGraphics;
using CoreAnimation;
using System.Threading.Tasks;
using Contacts;
using System.Collections.Generic;
using ContactsUI;
using System.Threading;
using Acr.UserDialogs;
using IPDFCameraViewControllerBinding;

namespace PicTap
{
    public partial class IPDFViewController : UIViewController
    {
		ImageReader ImageHelper = new ImageReader();
		UIImageView captureImageView = new UIImageView();
		UITapGestureRecognizer dismissTap;
		bool adjustImageFrame = false;

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

		partial void TestButton_TouchUpInside(UIButton sender)
		{
			ImageHelper.RunTest(9);
		}

		public IPDFViewController() : base ("IPDFViewController", null)
		{
			weakSelf = new WeakReference(this);
			if (weakSelf == null) Console.WriteLine("weakSelf is null"); else Console.WriteLine("not null");
		}

        public IPDFViewController (IntPtr handle) : base (handle)
        {
        }

		public override bool PrefersStatusBarHidden()
		{
			return true;
		}

		public override async void ViewDidLoad()
		{
			base.ViewDidLoad();

			GlobalVariables.VCToInvokeOnMainThread = this;
			//(iOSNavigationHelper.GetUINavigationController() as UINavigationController).SetNavigationBarHidden(true, false);

			weakSelf = new WeakReference(this);
			if (weakSelf == null) Console.WriteLine("weakSelf is null"); else Console.WriteLine("not null");

			ipdfView.SetupCameraView();
			ipdfView.EnableBorderDetection = true;
			ipdfView.EnableTorch = false;
			ipdfView.CameraViewType = IPDFCameraViewType.Normal;
			dismissTap = null;

			//AdFactory.AdjustViewControllerLayoutForAds(this);
			//this.SetCanDisplayBannerAds(true)
		}

		public override void ViewDidAppear(bool animated)
		{
			Console.WriteLine("ViewDidAppear");
			base.ViewDidAppear(animated);

			ipdfView.Start();
			//UserInteractionHelper.CheckIfPremiumShowAdsIfNot();
			//UserInteractionHelper.StoreUserEmail();
		}

		partial void ChoosePhotoButton_TouchUpInside(UIButton sender)
		{
			ChoosePhoto();
		}

		async Task ChoosePhoto() { 
			var photoStream = await PhotoPickerService.ChoosePicture();
			if (photoStream != null) {
				loadContactsFromPic(StreamByteDataUIImageConverter.GetUIImageFromStream(photoStream), true);
			}
		}

		void EnableDisableUI() { 
			CaptureBtn.Enabled = !CaptureBtn.Enabled;
			ChoosePhotoButton.Enabled = !ChoosePhotoButton.Enabled;
			FlashButton.Enabled = !FlashButton.Enabled;
		}

		public async void loadContactsFromPic(UIImage transformedcropped, bool saveProcessedImage)
		{
			Console.WriteLine("In loadContactsFromPic");
			EnableDisableUI();

			var internetStatus = Reachability.InternetConnectionStatus();
			if (internetStatus == NetworkStatusType.NotReachable){
				Console.WriteLine("No internet connection available, using Tesseract");

				await ImageHelper.ReadBusinessCardThenSaveExport_Tesseract(transformedcropped, 
				                                                           ProgressBar, loadingView);
			}
			else {
				Console.WriteLine("Internet connection available, using Microsoft Vision");

				/*await ImageHelper.ReadBusinessCardThenSaveExport_Tesseract(
					transformedcropped, ProgressBar, loadingView, true);*/

				/*await ImageHelper.ReadBusinessCardThenSaveExportHandleTimeout_MicrosoftVision(
					transformedcropped,
					ProgressBar, loadingView);*/ //fix task cancellation

				await ImageHelper.ReadBusinessCardThenSaveExport_MicrosoftVision(
					StreamByteDataUIImageConverter.GetStreamFromUIImage(transformedcropped), 
					ProgressBar, loadingView, true);
			}

			EnableDisableUI();
			Console.WriteLine("loadContactsFromPic Done");
		}

		/*void focusGesture(UITapGestureRecognizer sender)
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
		}*/

		/*void updateTitleLabel()
		{
			CATransition animation = new CATransition();
			animation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseInEaseOut);
			animation.Type = CAAnimation.TransitionPush;
			animation.Subtype = CAAnimation.TransitionFromBottom;
			animation.Duration = 0.35;
			this.titleLabel.Layer.AddAnimation(animation, CAAnimation.TransitionFade);

			string detectMode =
				(this.ipdfView.CameraViewType == IPDFCameraViewType.BlackAndWhite) ? "TEXT FILTER" : "COLOR FILTER";
			
			this.titleLabel.Text = detectMode +
				((this.ipdfView.EnableBorderDetection) ? "AUTOCROP On" : "AUTOCROP Off");
		}*/

		void changeFlashButton(bool enable)
		{
			FlashButton.SetImage(UIImage.FromFile(
				enable ? "flash.png" : "flashoff.png"),
					UIControlState.Normal);
		}

		/*void changeDetectButton(NSString title) { 
			FilterButton.SetImage(UIImage.FromFile(
				(title.Contains(singleDetect)) ? "singledetect.png" : "multidetect.png"),
				UIControlState.Normal);
		}*/

		void dismissPreview(UITapGestureRecognizer dismissTapParam)
		{
			Console.WriteLine("dismiss tap");
			UIView.AnimateNotify(0.9, 0, (nfloat)0.8, (nfloat)0.6, UIViewAnimationOptions.AllowUserInteraction,
				() =>
				{
					//dismissTapParam.View.Frame = CGRect.Inflate(this.View.Bounds, 0, this.View.Bounds.Size.Height);
					dismissTapParam.View.Frame = new CGRect(WeakSelf.View.Bounds.X, -WeakSelf.View.Bounds.Height,
													WeakSelf.View.Bounds.Width, WeakSelf.View.Bounds.Height);
				},
			     (bool finished)=> {
					 dismissTapParam.View.RemoveFromSuperview();
				}
			);
		}

		/*public async Task SaveImage()
		{
			var filename = System.DateTime.Now.Second + "cropped.png";
			Console.WriteLine("Saving Image, {0}", filename);
			DeviceUtil.SaveImageToPhotosApp(captureImageView.Image,
				filename);
			Console.WriteLine("Done with IPDFCamera.SaveImage()");
		}*/

		/*void focusIndicatorAnimateToPoint(CGPoint targetPoint)
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
		}*/

		/*partial void FilterButton_TouchUpInside(UIButton sender)
		{
			this.ipdfView.CameraViewType = (this.ipdfView.CameraViewType == IPDFCameraViewType.BlackAndWhite) ?
				IPDFCameraViewType.Normal : IPDFCameraViewType.BlackAndWhite;
			//this.updateTitleLabel();
		}*/

		partial void FlashButton_TouchUpInside(UIButton sender)
		{
			try
			{
				bool enable = !this.ipdfView.EnableTorch;
				//NSString text = (enable) ? flashOn : flashOff;
				this.changeFlashButton(enable);
				this.ipdfView.EnableTorch = enable;
			}
			catch (Exception e)
			{
				Console.WriteLine("IPDFCameraViewController.ipdfView.torchToggle error: {0}", e.Message);
			}
		}

		partial void CaptureBtn_TouchUpInside(UIButton sender)
		{
			this.ipdfView.CaptureImageWithCompletionHander(new Action<NSString>((NSString imageFilePath) =>
			{
				captureImageView = new UIImageView(new UIImage(imageFilePath));

				captureImageView.BackgroundColor = 
					UIColor.Black.ColorWithAlpha((nfloat)0.7);
				
				captureImageView.Frame = new CGRect(WeakSelf.View.Bounds.X, -WeakSelf.View.Bounds.Height,
				                                    WeakSelf.View.Bounds.Width, WeakSelf.View.Bounds.Height);             

				captureImageView.Alpha = (nfloat)1.0;
				captureImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
				captureImageView.UserInteractionEnabled = true;
				WeakSelf.View.AddSubview(captureImageView);

				dismissTap = new UITapGestureRecognizer(dismissPreview);
				captureImageView.AddGestureRecognizer(dismissTap);
					
				UIView.AnimateNotify(UIAccessibility.IsReduceMotionEnabled ? 0.0 : 0.8, 
				     0.0, 
				    (nfloat)1.0, 
				    (nfloat)0.5,
				    UIViewAnimationOptions.AllowUserInteraction, 
					() =>
					{
						captureImageView.Frame = WeakSelf.View.Bounds;
					}, 
				    (bool finished) => { 
						Console.WriteLine("Capture image finished: {0}", finished);
						if (finished)
						{
							loadContactsFromPic(captureImageView.Image, true);
						}
						else UserDialogs.Instance.Alert("Please try again", "Something went wrong", "OK");
					}
				);
			}));
		}
	}
}