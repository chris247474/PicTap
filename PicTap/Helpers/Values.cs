using System;

namespace PicTap
{
	public static class Values
	{
		public const string APPNAME = "PicTap";



		public const string PICCROPPED = "cropped";
		public const string IMPORTPIC = "Take a picture";

		public const string WARM = "Warm";
		public const string COLD = "Cold";
		public const string SEMIWARM = "Semi Warm";

		public const string DISABLEAUTOCALLTOUCHEVENT = "disableautocalltouchevent";
		public const string ENABLEABLEAUTOCALLTOUCHEVENT = "enableautocalltouchevent";

		public const string UNFOCUSPLAYLISTPAGESEARCHBAR = "unfocussearchbar";

		public const string READYFOREXTRATIPS = "readyforextratips";
		public const string DONEADDINGCONTACT = "doneaddingcontact";
		public const string DONEWAUTOCALLTIP = "donewautocalltip";

		public const string CAPPTUTORIALCOLOR_Green = "#004D26";//"#388E3C";
		public const string CAPPTUTORIALCOLOR_Orange = "#DE6C00";//"#388E3C";
		public const string MaterialDesignOrange = "#F57C00";
		public const string CAPPTUTORIALCOLOR_Blue = "##2196F3";//"#388E3C";
		public const string CAPPTUTORIALCOLOR_Purple = "#9C27B0";//"#388E3C";
		public const string CAPPTUTORIALCOLOR_LIGHTORANGE = "#FFB24A";
		public const string DARKBLUENAVBAR = "#1976D2";
		public const string YELLOW = "#FFC107";

		public const string FABNORMALCOLOR = "#FF9800";
		public const string FABPRESSEDCOLOR = "#3F51B5";

		public const string WARMCONTACTS = "warm";
		public const string COLDCONTACTS = "cold";

		public const string INFOTOAST = "INFO";
		public const string SUCCESSTOAST = "SUCCESS";
		public const string ERRORTOAST = "ERRROR";

		public const string IMPORTCHOICEMANUAL = "Enter Manually";
		public const string IMPORTCHOICEGDRIVE = "Google Drive";

		public const int CALLTOTEXTDELAY = 4000;

		public const string BACKGROUNDLIGHTSILVER = "#F5F6F7";
		public const string GOOGLEBLUE = "#4285F4";
		public const string TESTCOLOR = "#512DA8";
		public const string PIVOTALNAVBLUE = "#1C4D76";
		public const string PURPLE = "#512DA8";
		public const string CYAN = "#00BCD4";
		public const string TEAL = "#009688";
		public const string ORANGE = "#FF9800";
		public const string STACKVIEWSDARKERPURPLE = "#944A7F";
		public const string STACKVIEWSPURPLE = "#9F527E";
		public const string STACKVIEWSORANGE = "#F1AC81";
		public const string STACKVIEWSDARKERCYANBLUE = "#2C7FAB";
		public const string STACKVIEWSCYANBLUE = "#2A93B3";
		public const string STACKVIEWSCYAN = "#26CFCE";

		public const string BACKGROUNDPURPLEGRADIENT = "#7C6E9F";
		public const string BACKGROUNDDARKPURPLEGRADIENT = "#1E1F47";

		public static string ApplicationURL = @"https://secretfiles.azurewebsites.net";

		public static string ISEDITING = "isediting";
		public static string DONEEDITING = "doneediting";

		public static string TODAY = "today";
		public static string TOMORROW = "tomorrow";
		public static string MEETINGSREMINDED = "reminded";
		public static string MEETINGSNOTYETREMINDED = "notreminded";
		public static string CONFIRM = "confirm";
		public static string BOM = "bom";

		public static string DONEWITHCALL = "Done";
        public static string iOSDONEWITHCALL = "iOSDone";
        public static string DONEWITHNOCALL = "DoneNoCall";

		public const string TODAYSCALLS = "Today's Calls";
		public const int TOMORROWMEETINGREMINDTIME = 3;

		public const bool MEASUREEXECUTION = true;

		public const string PURCHASED = "purchased";
		public const string PRESENTED = "presented";
		public const string APPOINTED = "appointed";
		public const string CALLED = "called";
		public const string NEXT = "next";
		public const string APPOINTMENTDESCRIPTIONBOM = "Appointed for BOM";
		public const string FOLLOWUP = "Follow up";
		public const double MEETINGLENGTH = 1.5;
		public const string NEXTMEETINGDEFAULT = "-1";
		public const int _5PMBOM = 17;

		public const string NAMENUMORGNOTESREGEX = "(([A-Z]*[a-z]*)(\\s*))*((\\()*(\\))*(\\d+)*(\\s)*(-)*(\\d+)*(-)*)*(\\w*\\s*)*";
		public const string OLDNAMENUMREGEX = "(([A-Z]*[a-z]*)(\\s*))*((\\()*(\\))*(\\d+)*(\\s)*(-)*(\\d+)*(-)*)*";
		public const string NAMENUMREGEX = "(([A-Z]*[a-z]*)(\\s*))*((\\()*(\\))*(\\d+)*([A-Z]*[a-z]*)(\\s)*(-)*)*(\\d)";
		public const string NUMREGEX = "((\\()*(\\))*(\\d+)*(\\s)*(-)*(\\d+)*(-)*)*";
		public const string CALLABLENUMREGEX = "9((\\()*(\\))*(\\d+)*(\\s)*(-)*(\\d+)*(-)*)*";
		public const string STRICTNUMREGEX = "9\\d\\d\\d\\d\\d\\d\\d\\d\\d";
		public const string WORDREGEX = "(([A-Z]*[a-z]*)(\\s*))*";
		public const string ANYSTRINGREGEX = "(\\w*\\s*)*";
		public const string INVALIDINBETWEENNUMREGEX = "(([A-Z]*(\\(*\\)*)[a-z]*)(\\s*)\\-*)*";
		//SINGLESPECIALCHARREGEX doesnt detect "_" char cause of xamarin studio compiler confusion (doesnt detect, causes error) 
		public const string SINGLESPECIALCHARREGEX =
			"(\\(*\\)*\\**\\'*\\`*\\~*\\.*\\,*\\?*\\:*\\;*\\\\*\\/*\\[*\\]*\\{*\\}*\\<*\\>*\\!*\\@*\\&*\\^*\\&*\\%*\\$*\\#*\\|*\\-*\\=*\\\"*)";//\+*
																										//"(\\(*\\)*\\**\\'*\\`*\\~*\\.*\\,*\\?*\\:*\\;*\\\\*\\/*\\[*\\]*\\{*\\}*\\<*\\>*\\+*\\!*\\@*\\&*\\^*\\&*\\%*\\$*\\#*\\|*\\-*\\=*\\\"*)";
		public const string COMPLETESINGLESPECIALCHARREGEX = "\\(*\\)*\\**\\'*\\`*\\~*\\.*\\,*\\?*\\:*\\;*\\\\*\\/*\\[*\\]*\\{*\\}*\\<*\\>*\\+*\\!*\\@*\\&*\\^*\\&*\\%*\\$*\\#*\\|*\\-*\\=*\\\"*\\ *";
		public const string COUNTRYCODE = "\\+63";
		public const string WEIRDCHARFL = "ﬂ";
		public const string LONGDASH = "—";
		public const string WEIRDQUOTE = "‘";
		public const string OTHERWEIRDQUOTE = "’";
		public const string UNDERLINE = "_";
		public const string WEIRDCHARFI = "ﬁ";

		public const string FNAMEPARAM = "fname";
		public const string LNAMEPARAM = "lname";
		public const string NUMBERPARAM = "num";
		public const string AFFPARAM = "aff";
		public const string NOTESPARAM = "notes";

		public const string NODUPLICATES = "";

		public const string ALLPLAYLISTPARAM = "All";

		//image preprocessing values. adjust to change image OCR readability. so far 28/30 perfect reads w current values
		public const double GAUSSIANSIGMA = 0;// 0 so far best value for OCR to read edges of font accurately
		public const double GAUSSIANSIZEX = 3;//3 usually //must be odd no diff when using 51
		public const double GAUSSIANSIZEY = 3;//3 usually //must be odd no diff when using 51
		public const double SHARPENMASKWEIGHT = 1;//1 gives better OCR readability then 2, 1.5, 0.8, 0.9

		public const double ADDWEIGHT = 1;

		public const int ADAPTIVETHRESHBLOCKSIZE = 13;//Must be odd
		public const int ADAPTIVETHRESHPARAM = 10;
	}
}