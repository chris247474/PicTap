using System;
namespace PicTap
{
	public class MatchFormat
	{
		public string[] Matches { get; set; }
		public string RemainingNonMatches { get; set; }

		public MatchFormat(string[] matches, string remaining) {
			Matches = matches;
			RemainingNonMatches = remaining;
		}

		public bool MatchFound() {
			var match = false;
			if (Matches.Length > 0)
			{
				for (int c = 0; c < Matches.Length; c++)
				{
					if (!string.IsNullOrWhiteSpace(Matches[c]))
					{
						match = true;
					}
				}
			}
			else return false;

			if (string.IsNullOrWhiteSpace(RemainingNonMatches)) return false;

			return match;
		}
	}
}

