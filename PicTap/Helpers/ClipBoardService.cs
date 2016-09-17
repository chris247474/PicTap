using System;
using UIKit;

namespace PicTap
{
	public static class ClipBoardService//also in DeviceUtil
	{
		public static void CopyToClipboard(String text)
		{
			UIPasteboard.General.String = text;
		}
	}
}

