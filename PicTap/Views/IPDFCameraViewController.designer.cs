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
    [Register ("IPDFCameraViewController")]
    partial class IPDFCameraViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton CaptureButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton CropButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton FilterButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton FlashButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView focusIndicator { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        IPDFCamera_Binding.IPDFCamera ipdfView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel titleLabel { get; set; }


        [Action ("CaptureButton_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void CaptureButton_TouchUpInside (UIButton sender);


        [Action ("CropButton_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void CropButton_TouchUpInside (UIButton sender);


        [Action ("FilterButton_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void FilterButton_TouchUpInside (UIButton sender);


        [Action ("FlashButton_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void FlashButton_TouchUpInside (UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (CaptureButton != null) {
                CaptureButton.Dispose ();
                CaptureButton = null;
            }

            if (CropButton != null) {
                CropButton.Dispose ();
                CropButton = null;
            }

            if (FilterButton != null) {
                FilterButton.Dispose ();
                FilterButton = null;
            }

            if (FlashButton != null) {
                FlashButton.Dispose ();
                FlashButton = null;
            }

            if (focusIndicator != null) {
                focusIndicator.Dispose ();
                focusIndicator = null;
            }

            if (ipdfView != null) {
                ipdfView.Dispose ();
                ipdfView = null;
            }

            if (titleLabel != null) {
                titleLabel.Dispose ();
                titleLabel = null;
            }
        }
    }
}