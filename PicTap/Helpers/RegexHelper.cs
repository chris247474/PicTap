using System;
using System.Text.RegularExpressions;

namespace PicTap
{
	public static class RegexHelper
	{
		public static string subtractFromString(string stringToRemove, string originalString)
		{
			return originalString.Replace(stringToRemove, "");
		}
		public static string RemoveCountryCodeAndSpecialChar(string input, Regex countryCode, Regex special)
		{
			return countryCode.Replace(special.Replace(input.Replace(Values.WEIRDCHARFL, "").Replace(Values.WEIRDQUOTE, "").Replace(Values.LONGDASH, "").Replace(Values.UNDERLINE, "").Replace(Values.WEIRDCHARFI, "").Replace(Values.OTHERWEIRDQUOTE, "").Trim(), ""), "0");
		}

		public static string RemoveMatchingString(string input, Regex special)
		{
			return special.Replace(input, "");
		}
		public static string[] SeparateFullNameIntoFirstAndLast(string rawName)
		{
			string firstname = "", lastname = "";
			var wordReg = new Regex(Values.WORDREGEX);
			var specialReg = new Regex(Values.SINGLESPECIALCHARREGEX);
			Match wordMatch = wordReg.Match(RemoveCountryCodeAndSpecialChar(
				rawName, new Regex(Values.COUNTRYCODE), specialReg));
			//Match wordMatch = wordReg.Match(rawName);

			Console.WriteLine("[SeparateFullNameIntoFirstAndLast] ABOUT TO PROCESS NAME, strictnummatch w " + rawName + " is " + wordMatch.Success.ToString());

			if (wordMatch.Success)
			{
				Console.WriteLine("[SeparateFullNameIntoFirstAndLast] wordMatch: " + wordMatch.Groups[0].Value);

				//processing name
				string[] words = wordMatch.Groups[0].Value.Split(new char[] { ' ', '\t' });
				Console.WriteLine("words split by spaces count: " + words.Length);
				if (words.Length > 1)
				{
					firstname = words[0];
					for (int i = 1; i < words.Length; i++)
					{
						lastname += words[i];
					}
				}
				else {
					//usually happens in OCR misreads. add to error list for user to manually correct
					lastname = words[0];
					firstname = " ";
					//errorStatus = 1;
				}
				Console.WriteLine("[SeparateFullNameIntoFirstAndLast] firstname: " + firstname + "lastname: " + lastname);

				return new string[] { firstname, lastname };
			}

			return null;
		}

		public static bool isValidMobile(string number)
		{
			Regex strictNumReg = new Regex(Values.STRICTNUMREGEX);
			Match strictNumMatch = strictNumReg.Match(number);
			if (!strictNumMatch.Success)
			{
				return false;
			}
			return true;
		}
	}
}

