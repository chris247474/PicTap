using System;

using UIKit;
using CoreGraphics;
using CoreAnimation;
using IPDFCamera_Binding;
using Foundation;
using System.Threading.Tasks;

namespace PicTap
{
	public partial class IPDFCameraViewController : UIViewController
	{
		ImagePreProcessor ImageHelper = new ImagePreProcessor();
		UIImageView captureImageView = new UIImageView();

		WeakReference weakSelf;
		IPDFCameraViewController WeakSelf{
			get
			{
				if (weakSelf ==null || !weakSelf.IsAlive)
					return null;
				return weakSelf.Target as IPDFCameraViewController;
			}
		}

		public IPDFCameraViewController () : base ("IPDFCameraViewController", null)
		{
			weakSelf = new WeakReference(this);
		}

		protected IPDFCameraViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			ipdfView.SetupCameraView ();
			ipdfView.EnableBorderDetection = true;		
		}

		public override void ViewDidAppear(bool animated){
			Console.WriteLine("ViewDidAppear");
			base.ViewDidAppear(animated);

			UITapGestureRecognizer focusTap = new UITapGestureRecognizer(focusGesture);
			ipdfView.AddGestureRecognizer(focusTap);

			ipdfView.Start ();
		}

		UIStatusBarStyle preferredStatusBarStyle()
		{
			return UIStatusBarStyle.LightContent;
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}

		void focusGesture(UITapGestureRecognizer sender){
			Console.WriteLine("in focusGesture");
			if (sender.State == UIGestureRecognizerState.Recognized) {
				Console.WriteLine("recognized");
				CGPoint location = sender.LocationInView (this.ipdfView);

				this.focusIndicatorAnimateToPoint (location);

				this.ipdfView.FocusAtPoint (location, () => {
					this.focusIndicatorAnimateToPoint(location);
				});
			}
		}

		void updateTitleLabel(){
			CATransition animation = new CATransition ();
			animation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseInEaseOut);
			animation.Type = CATransition.TransitionPush;
			animation.Subtype = CATransition.TransitionFromBottom;
			animation.Duration = 0.35;
			this.titleLabel.Layer.AddAnimation (animation, CATransition.TransitionFade);

			string filterMode = 
				(this.ipdfView.CameraViewType == IPDFCameraViewType.BlackAndWhite) ? "TEXT FILTER": "COLOR FILTER";
			/*if (this.ipdfView.EnableBorderDetection) {
				this.titleLabel.Text = (NSString)@"AUTOCROP On";
			} else {
				this.titleLabel.Text = (NSString)@"AUTOCROP Off";
			}*/
			this.titleLabel.Text = filterMode + " | " + 
				((this.ipdfView.EnableBorderDetection) ? "AUTOCROP On" : "AUTOCROP Off");
		}

		void changeButton(UIButton button, NSString title, bool enabled){
			button.SetTitle (title, UIControlState.Normal);
			if (enabled) {
				button.SetTitleColor (UIColor.FromRGB ((nfloat)1, (nfloat)0.81, (nfloat)0).ColorWithAlpha((nfloat)1), 
					UIControlState.Normal);
			} else {
				button.SetTitleColor (UIColor.White, UIControlState.Normal);
			}
		}

		void dismissPreview(UITapGestureRecognizer dismissTap){
			UIView.Animate (0.7, 0, UIViewAnimationOptions.AllowUserInteraction,
				() => {
					dismissTap.View.Frame = CGRect.Inflate(this.View.Bounds, 0, this.View.Bounds.Size.Height);
				},
				null
			);
		}

		public async Task SaveImage(){
			var filename = System.DateTime.Now.Second + "cropped.png";
			Console.WriteLine("Saving Image, {0}", filename);
			ImageHelper.SaveImageToPhotosApp/*SaveImageToDiskThenNotifyViewModelToStartPreprocessingImage*/(ImageHelper.GetStreamFromUIImage(captureImageView.Image), 
				filename);
			Console.WriteLine ("Done with IPDFCamera.SaveImage()");
		}

		void focusIndicatorAnimateToPoint(CGPoint targetPoint){
			this.focusIndicator.Center = targetPoint;
			this.focusIndicator.Alpha = (nfloat)0.0;
			this.focusIndicator.Hidden = false;

			UIView.Animate (0.4,
				() => {
					this.focusIndicator.Alpha = (nfloat)0.5;
				},
				()=>{
					UIView.Animate(0.4, ()=>{
						this.focusIndicator.Alpha = (nfloat)0.0;
					});
				}
			);
		}

		partial void FlashButton_TouchUpInside (UIButton sender/*, EventArgs ea*/)
		{
			try{
				bool enable = !this.ipdfView.EnableTorch;
				NSString text = (enable) ? (NSString)@"FLASH On" : (NSString)@"FLASH Off";
				this.changeButton (sender as UIButton, text, enable);
				this.ipdfView.EnableTorch = enable;
			}catch(Exception e){
				Console.WriteLine ("IPDFCameraViewController.ipdfView.torchToggle error: {0}", e.Message);
			}		
		}

		partial void CaptureButton_TouchUpInside (UIButton sender)
		{


		}

		partial void FilterButton_TouchUpInside (UIButton sender)
		{
			this.ipdfView.CameraViewType = (this.ipdfView.CameraViewType == IPDFCameraViewType.BlackAndWhite) ? 
				IPDFCameraViewType.Normal : IPDFCameraViewType.BlackAndWhite;
			this.updateTitleLabel ();		
		}

		partial void CropButton_TouchUpInside (UIButton sender)
		{
			/*try{
				bool enable = !this.ipdfView.EnableBorderDetection;
				NSString text = (enable) ? (NSString)@"CROP On" : (NSString)@"CROP Off";
				this.changeButton (sender as UIButton, text, enable);
				this.updateTitleLabel();
			}catch(Exception e){
				Console.WriteLine ("IPDFCameraViewController.ipdfView.borderDetectToggle error: {0}", e.Message);
			}*/	
		}
	}
}


