using System;
using System.Text.RegularExpressions;

namespace PicTap
{
	public static class RegexHelper
	{
		//public const string EMAILREGEXV2 = 
		//	"^([\\w-\\.]+@(?!gmail.com)(?!yahoo.com)(?!hotmail.com)([\\w- ]+\\.)+[\\w-]{2,4})?$";
		public const string LABELEDEMAILREGEX =
			"(\\s*[A-Z]*[a-z]*.*:\\s*)*\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*";//"(\s*[A-Z]*[a-z]*:\s*)*\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
		public const string EMAILREGEX = "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*";
		public const string ALLNUMBERSREGEX = "((\\()*(\\))*(\\d+)*(\\s)*(-)*(\\d+)*(-)*)*";
		public const string SINGLENUMREGEX = "((\\+)*(\\()*(\\))*(\\d+)*(\\s)*(-)*(\\d+)*(-)*)*";
		public const string STRICTNUMREGEX = "((\\+)*(\\d)*(\\s)*)*";
		public const string MULTILABELEDNUMREGEX = //does not detect multiple numbers in the same line (need to add *)
			"(\\s*[A-Z]*[a-z]*.*:\\s*)*((\\+)*(\\()*(\\))*(\\d)+(\\d)+(\\d)+(\\d)+(\\d)+(\\s)*(\\-)*)+";
		//"(\\s)*([A-Z]*[a-z]*(\\:)*(\\s)*)*(\\s)*((\\+)*(\\()*(\\))*(\\d+)*(\\s)*(-)*(\\d+)*(-)*)*";
		//"(\\s)*((\\w)*(\\s)*(\\:)*)((\\+)*(\\()*(\\))*(\\d+)*(\\s)*(-)*(\\d+)*(-)*)*";
		public const string LABELREGEX = "(\\s*[A-Z]*[a-z]*.*:\\s*)*";
			//"(\\s)*([A-Z]*[a-z]*(\\:)*(\\s)*)*";
			//"(\\s)*([A-Z]*[a-z]*(\\:)*(\\s)*)*(\\s)*";//"(\\s)*((\\w)*(\\s)*(\\:)*)(\\s)*";
		public const string SINGLENAMEREGEX = "([A-Z]*[a-z]*)(\\\\s*)*";
		public const string PERSONSNAMEREGEX = "(([\\p{L}'-]+)\\s)([A-Z]*[a-z]*.\\s)*([\\p{L}'-]+)";
		public const string ORGREGEX = //"([\\p{L}'-]+)";
			//need to improve differentiation between org and name - for now, 
			//org only recognizes text followed by substrings of const COMPANYSTRINGS
			"(\\w+\\s*((Corp)|(Org)|(Company)|(Inc)))|([A-Z]{3})";//|([\\p{L}'-]+)";
		public const string ADDRESSREGEX = 
			LABELREGEX+
			"(\\d{1,3}.?\\d{0,3}\\s[a-zA-Z]{2,30}(\\s[a-zA-Z]{2,15})?([#\\.0-9a-zA-Z]*)?(\\,*\\s\\w*\\,)*(\\s)*(City)*(\\s)*(\\d{4,7})*)";
		//"(([A-Z]*[a-z]*)(\\s*)*)*";
		public const string STREETREGEX = "\\d{1,3}.?\\d{0,3}\\s[a-zA-Z]{2,30}(\\s[a-zA-Z]{2,15})?([#\\.0-9a-zA-Z]*)?";
		public const string CITYREGEX = "([A-z]*\\s*[a-z]*)*\\sCity";
		public const string COUNTRYREGEX = "";
		public const string POSTALREGEX = "\\d{4,7}";
		public const string CITYPOSTALREGEX = "(\\d{4,7}\\sCity)*(City\\s\\d{4,7})*";
		public const string WWW_LIMITEDURLREGEX =
			"((www|(http|https|ftp|news|file)+\\:\\/\\/)*[_.a-z0-9-]+\\.[a-z0-9\\/_:@=.+?,##%&~-]*[^.|\\'|\\# |!|\\(|?|,| |>|<|;|\\)])";
			//"((www\\.|(http|https|ftp|news|file)+\\:\\/\\/)[_.a-z0-9-]+\\.[a-z0-9\\/_:@=.+?,##%&~-]*[^.|\\'|\\# |!|\\(|?|,| |>|<|;|\\)])";
		//public const string COM_LIMITEDREGEX = "(http(s)?:)?([\\w-]+\\.)+[\\w-]+[.com]+([?%&=]*)?";
		public const string COPYRIGHTCHAR = "©";
		public const string COMSTRING = "com";
		public const string DOTCOMSTRING = "." + COMSTRING;
		public const string ATSTRING = "@";
		public const string EMAILTESSERACTERRORSREGEX = 
			"(\\**\\'*\\`*\\~*\\?*\\:*\\;*\\\\*\\/*\\[*\\]*\\{*\\}*\\<*\\>*\\+*\\!*\\@*\\&*\\^*\\&*\\%*\\$*\\#*\\=*" +
			"\\\"*)";
		public const string STRAIGHTVERTICALLINECHAR = "|";
		public const string LETTERLLOWERCASE = "l";
		public const string COMMA = ",";
		public const string DOT = ".";
		public const string DASH = "-";
		public const string WWWSTRING = "www";
		public const string WWWDOTSTRING = "www.";
		public const string COMPANYSTRINGS = 
			"COMPANY CORP ORG INC INDUSTRIES TELECOM BANK PHILIPPINES LAND TECHNOLOGY CONSULTANCY ELECTRON FOOD COMPUTERS" +
			"EXPRESS DELIVERY BOOKS GROUP BUSINESS PREMIER GYM BOXING OFFICE";
		public const string JOBTITLES = "manager associate president assistant secretary operations ceo coo cto cpa" +
			"lawyer consultant director marketing specialist trainee trainer teacher professor doctor Dr. broker attorney designer" +
			"planner executive prc";
		public const string CITYLABELS = "city";
		public const string STREETLABELS = "st st. ave avenue road loop alley";


		public static string subtractFromString(string stringToRemove, string originalString)
		{
			return originalString.Replace(stringToRemove, "");
		}
		public static string RemoveCountryCodeReadingErrorsAndSpecialChar(string input, Regex countryCode = null, Regex special = null)
		{
			special = new Regex(Values.SINGLESPECIALCHARREGEX);
			//countryCode = new Regex(Values.COUNTRYCODE);
			//return countryCode.Replace(
			return special.Replace(
				input.Replace(Values.WEIRDCHARFL, "").Replace(Values.WEIRDQUOTE, "").Replace(
					Values.LONGDASH, "").Replace(Values.UNDERLINE, "").Replace(
						Values.WEIRDCHARFI, "").Replace(Values.OTHERWEIRDQUOTE, "").Trim(), "");//, "0");
		}

		public static string RemoveCommonTesseractEmailErrors(string input) {
			//var special = new Regex(EMAILTESSERACTERRORSREGEX);

			var filteredInput = input.Replace(COPYRIGHTCHAR, ATSTRING);
			Console.WriteLine("Replaced copyright misread: {0}", filteredInput);

			if (!filteredInput.Contains(DOTCOMSTRING)) { 
				filteredInput = string.IsNullOrWhiteSpace(filteredInput)
								  ? input.Replace(COMSTRING, DOTCOMSTRING) :
				filteredInput.Replace(COMSTRING, DOTCOMSTRING);
				Console.WriteLine("Replaced .com misread: {0}", filteredInput);
			}

			filteredInput = string.IsNullOrWhiteSpace(filteredInput) ? input.Replace(STRAIGHTVERTICALLINECHAR
			                                                                         , LETTERLLOWERCASE) :
				filteredInput.Replace(STRAIGHTVERTICALLINECHAR, LETTERLLOWERCASE);
			Console.WriteLine("Replaced letter l misread: {0}", filteredInput);

			return filteredInput;
		}

		public static string RemoveCommonTesseractURLErrors(string input)
		{
			//var special = new Regex(EMAILTESSERACTERRORSREGEX);

			var filteredInput = input;//.Replace(COPYRIGHTCHAR, ATSTRING);
			//Console.WriteLine("Replaced copyright misread: {0}", filteredInput);

			filteredInput = string.IsNullOrWhiteSpace(filteredInput) ? input.Replace(COMSTRING, DOTCOMSTRING) :
				filteredInput.Replace(COMSTRING, DOTCOMSTRING);
			Console.WriteLine("Replaced .com misread: {0}", filteredInput);

			filteredInput = string.IsNullOrWhiteSpace(filteredInput) ? input.Replace(COMMA, DOT) :
				filteredInput.Replace(COMMA, DOT);
			Console.WriteLine("Replaced comma misread: {0}", filteredInput);

			filteredInput = string.IsNullOrWhiteSpace(filteredInput) ? input.Replace(STRAIGHTVERTICALLINECHAR
																					 , LETTERLLOWERCASE) :
				filteredInput.Replace(STRAIGHTVERTICALLINECHAR, LETTERLLOWERCASE);
			Console.WriteLine("Replaced letter l misread: {0}", filteredInput);

			return filteredInput;
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
			Match wordMatch = wordReg.Match(RemoveCountryCodeReadingErrorsAndSpecialChar(
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

