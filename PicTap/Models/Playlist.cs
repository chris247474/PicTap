

using PicTap;
using SQLite;

namespace PicTap
{
	[Table("Playlists")]
	public class Playlist : BaseViewModel
	{
		[PrimaryKey, AutoIncrement, Column("ID"), NotNull]
		public int ID { get; set; }

		string _playlistname;
		[Column("PlaylistName"), NotNull, Unique]
		public string PlaylistName
		{
			get { return _playlistname; }
			set { SetProperty(ref _playlistname, value, nameof(PlaylistName)); }
		}

		string _icon = "people.png";
		[Column("Icon")]
		public string Icon
		{
			get { return _icon; }
			set { SetProperty(ref _icon, value, nameof(Icon)); }
		}

		int _lastindexcalled = 0;
		[Column("LastIndexCalled")]
		public int LastIndexCalled
		{
			get
			{
				return _lastindexcalled;
			}
			set
			{
				SetProperty(ref _lastindexcalled, value, nameof(LastIndexCalled));
			}
		}
	}
}

