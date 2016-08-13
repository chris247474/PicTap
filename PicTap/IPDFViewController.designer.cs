// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace PicTap
{
    [Register ("IPDFViewController")]
    partial class IPDFViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton CaptureBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton ChoosePhotoButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton FlashButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        IPDFCamera_Binding.IPDFCamera ipdfView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIActivityIndicatorView loadingView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIProgressView ProgressBar { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton testButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel titleLabel { get; set; }

        [Action ("FlashButton_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void FlashButton_TouchUpInside (UIKit.UIButton sender);

        [Action ("ChoosePhotoButton_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void ChoosePhotoButton_TouchUpInside (UIKit.UIButton sender);

        [Action ("CaptureBtn_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void CaptureBtn_TouchUpInside (UIKit.UIButton sender);

        [Action ("TestButton_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void TestButton_TouchUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (CaptureBtn != null) {
                CaptureBtn.Dispose ();
                CaptureBtn = null;
            }

            if (ChoosePhotoButton != null) {
                ChoosePhotoButton.Dispose ();
                ChoosePhotoButton = null;
            }

            if (FlashButton != null) {
                FlashButton.Dispose ();
                FlashButton = null;
            }

            if (ipdfView != null) {
                ipdfView.Dispose ();
                ipdfView = null;
            }

            if (loadingView != null) {
                loadingView.Dispose ();
                loadingView = null;
            }

            if (ProgressBar != null) {
                ProgressBar.Dispose ();
                ProgressBar = null;
            }

            if (testButton != null) {
                testButton.Dispose ();
                testButton = null;
            }

            if (titleLabel != null) {
                titleLabel.Dispose ();
                titleLabel = null;
            }
        }
    }
}