using System;
using UIKit;

namespace PicTap
{
	public static class ClipBoardService
	{
		public static void CopyToClipboard(String text)
		{
			UIPasteboard.General.String = text;
		}
	}
}

