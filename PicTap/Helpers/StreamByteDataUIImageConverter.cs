using System;
using System.IO;
using Foundation;
using UIKit;

namespace PicTap
{
	public static class StreamByteDataUIImageConverter
	{
		public static Stream GetStreamFromFilename(string file)
		{
			//Console.WriteLine ("In GetStreamFromFilename");
			return GetStreamFromUIImage(UIImage.FromFile(file));
		}
		public static Stream GetStreamFromUIImage(UIImage image)
		{
			//Console.WriteLine ("In GetStreamFromUIImage");
			return BytesToStream(UIImageToBytes(image));
		}
		public static UIImage GetUIImageFromStream(Stream s)
		{
			//Console.WriteLine ("in GetUIImageFromStream");
			return GetImagefromByteArray(StreamToBytes(s));
		}

		public static UIImage GetImagefromByteArray(byte[] imageBuffer)
		{
			//Console.WriteLine ("in GetImagefromByteArray");
			NSData imageData = NSData.FromArray(imageBuffer);
			//Console.WriteLine ("NSData loaded from bytes");
			var img = UIImage.LoadFromData(imageData);
			//Console.WriteLine ("UIImage null: {0}", (img == null) ? true : false);
			return img;
		}

		public static byte[] StreamToBytes(Stream input)
		{
			//Console.WriteLine ("In StreamToBytes");
			using (MemoryStream ms = new MemoryStream())
			{
				input.CopyTo(ms);
				//Console.WriteLine ("bytes copied");
				ms.Seek(0, SeekOrigin.Begin);
				//Console.WriteLine ("seekorigin done");
				input.Seek(0, SeekOrigin.Begin);
				//Console.WriteLine ("input seek done");
				return ms.ToArray();
			}
		}

		public static Stream BytesToStream(byte[] image)
		{
			//Console.WriteLine ("In BytesToStream");
			MemoryStream stream = new MemoryStream();
			stream.Write(image, 0, image.Length);
			stream.Seek(0, SeekOrigin.Begin);
			return stream;
		}

		public static byte[] UIImageToBytes(UIImage image)
		{
			Byte[] myByteArray = null;
			using (NSData imageData = image.AsPNG())
			{
				myByteArray = new Byte[imageData.Length];
				System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes, myByteArray, 0,
					Convert.ToInt32(imageData.Length));
			}
			return myByteArray;
		}
	}
}

