using System;
namespace PicTap
{
	public class NameMatchFormat
	{
		public string Name { get; set; }
		public string Remaining { get; set; }

		public NameMatchFormat(string name, string remaining) {
			Name = name;
			Remaining = remaining; 
		}
	}
}

