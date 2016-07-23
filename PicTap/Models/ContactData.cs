using System;
using SQLite;

namespace PicTap
{
	[Table ("Contacts")]
	public class ContactData:BaseViewModel
	{
		[PrimaryKey, AutoIncrement, Column("ID"), NotNull]
		public int ID { get; set; }

		string _name;
		[Column("Name"), NotNull]
		public string Name { 
			get
			{
				return _name;
			}
			set 
			{
				SetProperty(ref _name, value, nameof(Name));
			}
		}

		string _firstname;
		[Column("FirstName"), NotNull]
		public string FirstName { 
			get 
			{
				return _firstname;
			}
			set 
			{
				SetProperty(ref _firstname, value, nameof(FirstName));
			}
		}

		string _lastname;
		[Column("LastName"), NotNull]
		public string LastName {
			get 
			{
				return _lastname;
			}
			set
			{
				SetProperty(ref _lastname, value, nameof(LastName));
			}
		}

		string _aff;
		[Column("Affiliation")]
		public string Aff {
			get
			{
				return _aff;
			}
			set
			{
				SetProperty(ref _aff, value, nameof(Aff));
			}
		}

		string _number;
		[Column("Number"), NotNull]
		public string Number {
			get
			{
				return _number;
			}
			set
			{
				SetProperty(ref _number, value, nameof(Number));
			}
		}

		[Column("Number2")]
		public string Number2 { get; set; }

		[Column("Number3")]
		public string Number3 { get; set; }

		[Column("Number4")]
		public string Number4 { get; set; }

		[Column("Number5")]
		public string Number5 { get; set; }

		string _playlist;
		[Column("Playlist")]
		public string Playlist { 
			get
			{
				return _playlist;
			}
			set
			{
				SetProperty(ref _playlist, value, nameof(Playlist));
			}
		}

		string _oldplaylist;// = Values.TODAYSCALLSUNDEFINED;
		[Column("OldPlaylist")]
		public string OldPlaylist{
			get
			{
				return _oldplaylist;
			}
			set
			{
				SetProperty(ref _oldplaylist, value, nameof(OldPlaylist));
			}
		}

		DateTime _called;
		[Column("Called")]
		public DateTime Called{
			get
			{
				return _called;
			}
			set
			{
				SetProperty(ref _called, value, nameof(Called));
			}
		}

		DateTime _appointed;
		[Column("Appointed")]
		public DateTime Appointed{
			get
			{
				return _appointed;
			}
			set
			{
				SetProperty(ref _appointed, value, nameof(Appointed));
			}
		}

		DateTime _presented;
		[Column("Presented")]
		public DateTime Presented{
			get
			{
				return _presented;
			}
			set
			{
				SetProperty(ref _presented, value, nameof(Presented));
			}
		}

		DateTime _purchased;
		[Column("Purchased")]
		public DateTime Purchased{
			get
			{
				return _purchased;
			}
			set
			{
				SetProperty(ref _purchased, value, nameof(Purchased));
			}
		}

		string _nextmeetingID;
		[Column("NextMeetingID")]
		public string NextMeetingID{
			get
			{
				return _nextmeetingID;
			}
			set
			{

				SetProperty(ref _nextmeetingID, value, nameof(NextMeetingID));
			}
		}

		DateTime _nextcall;
		[Column("NextCall")]
		public DateTime NextCall{
			get
			{
				return _nextcall;
			}
			set
			{

				SetProperty(ref _nextcall, value, nameof(NextCall));
			}
		}

		bool _isselected;
		[Column("IsSelected")]
		public bool IsSelected{
			get
			{
				return _isselected;
			}
			set
			{
				SetProperty(ref _isselected, value, nameof(IsSelected));
			}
		}

		[Column("AzureID")]
		public string AzureID{ get; set;}

		string _smallpic;// = UIBuilder.ChooseRandomProfilePicBackground(
			//App.ProfileBackground);
		[Column("PicStringBase64")]
		public string PicStringBase64 {
			get
			{
				return _smallpic;
			}
			set
			{
				SetProperty(ref _smallpic, value, nameof(PicStringBase64));
			}
		}

		string _largepic;// = UIBuilder.ChooseRandomProfilePicBackground(
			//App.ProfileBackground);
		[Column("LargePic")]
		public string LargePic
		{
			get
			{
				return _largepic;
			}
			set
			{
				SetProperty(ref _largepic, value, nameof(LargePic));
			}
		}

		bool _isconfirmedtomorrow = false;
		[Column("IsConfirmedTomorrow")]
		public bool IsConfirmedTomorrow
		{
			get
			{
				return _isconfirmedtomorrow;
			}
			set
			{
				SetProperty(ref _isconfirmedtomorrow, value, nameof(IsConfirmedTomorrow));
			}
		}

		bool _isconfirmedtoday = false;
		[Column("IsConfirmedToday")]
		public bool IsConfirmedToday
		{
			get
			{
				return _isconfirmedtoday;
			}
			set
			{
				SetProperty(ref _isconfirmedtoday, value, nameof(IsConfirmedToday));
			}
		}

		string _initials;
		[Column("Initials")]
		public string Initials
		{
			get
			{
				return _initials;
			}
			set
			{
				SetProperty(ref _initials, value, nameof(Initials));
			}
		}

		public bool IsAppointed{
			get{ return (Appointed.Date == DateTime.MinValue) ? false : true; }
		}

		public bool IsSetForNextCall{
			get{ return (NextCall.Date == DateTime.MinValue) ? false : true; }
		}

		public bool ShouldCallToday{
			get{ return (NextCall.Date == DateTime.Today.Date) ? true : false; }
		}

		bool _usesDefaultImage;
		public bool HasDefaultImage_Large{
			get{ 
				_usesDefaultImage = false;
				if(LargePic.Contains("profile-")){
					_usesDefaultImage = true;
				}
				return _usesDefaultImage;
			}
		}

		public bool HasDefaultImage_Small{
			get{ 
				_usesDefaultImage = false;
				if(PicStringBase64.Contains("profile-")){
					_usesDefaultImage = true;
				}
				return _usesDefaultImage;
			}
		}
	}
}

