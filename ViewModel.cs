using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace HotkeyMusicPlayer
{
    class ViewModel : INotifyPropertyChanged
    {
        private const string folderPathFileName = "path.txt";
        private static readonly Random ran = new Random();

        private bool isAllSongsShuffle, isSearchShuffle, playOnlySearchSongs;
        private double sliderPercent = 0;
        private string searchKey, lowerSearchKey;
        private string[] sources;
        private Song currentSong;
        private Song[] allSongsShuffled, songs;
        private MediaElement player;

        public bool IsAllSongsShuffle
        {
            get { return isAllSongsShuffle; }
            set
            {
                if (value == isAllSongsShuffle) return;

                isAllSongsShuffle = value;

                OnNotifyPropertyChanged("IsAllSongsShuffle");
                OnNotifyPropertyChanged("IsShuffle");

                UpdateSongs();
            }
        }

        public bool IsSearchShuffle
        {
            get { return isSearchShuffle; }
            set
            {
                if (value == isSearchShuffle) return;

                isSearchShuffle = value;

                OnNotifyPropertyChanged("IsAllSongsShuffle");
                OnNotifyPropertyChanged("IsShuffle");

                UpdateSongs();
            }
        }

        public bool IsShuffle
        {
            get { return IsSearching ? IsSearchShuffle : IsAllSongsShuffle; }
            set
            {
                if (IsSearching) IsSearchShuffle = value;
                else IsAllSongsShuffle = value;
            }
        }

        public bool PlayOnlySearchSongs
        {
            get { return playOnlySearchSongs; }
            set
            {
                if (value == playOnlySearchSongs) return;

                playOnlySearchSongs = value;
                OnNotifyPropertyChanged("PlaySearchSongs");
            }
        }

        public bool IsSearching { get { return !string.IsNullOrEmpty(SearchKey); } }

        public string SearchKey
        {
            get { return searchKey; }
            set
            {
                if (value == searchKey) return;

                searchKey = value;
                lowerSearchKey = searchKey.ToLower();

                OnNotifyPropertyChanged("SearchKey");
                OnNotifyPropertyChanged("IsSearching");
                OnNotifyPropertyChanged("IsShuffle");

                UpdateSongs();
            }
        }

        public object CurrentShowSong
        {
            get { return Songs.Contains(currentSong) ? (object)currentSong : null; }
            set
            {
                if (value == null) return;

                Song song = (Song)value;
                if (song == currentSong || !allSongsShuffled.Contains(song)) return;

                currentSong = song;
                OnNotifyPropertyChanged("CurrentShowSong");
                OnNotifyPropertyChanged("CurrentPlaySong");
            }
        }

        public Song CurrentPlaySong
        {
            get { return currentSong; }
            set
            {
                if (value == currentSong || !allSongsShuffled.Contains(value)) return;

                currentSong = value;
                OnNotifyPropertyChanged("CurrentShowSong");
                OnNotifyPropertyChanged("CurrentPlaySong");
            }
        }

        public double CurrentPlaySongPosition
        {
            get { return sliderPercent; }
            set
            {
                if (value == sliderPercent) return;

                sliderPercent = value;
                player.Position = TimeSpan.FromDays(GetPlayerDuration().TotalDays * value);

                OnNotifyPropertyChanged("CurrentPlaySongPosition");
                OnNotifyPropertyChanged("CurrentPlaySongPositionText");
                OnNotifyPropertyChanged("CurrentPlaySongDurationText");
            }
        }

        public string CurrentPlaySongPositionText
        {
            get { return ConvertToString(player.Position); }
        }

        public string CurrentPlaySongDurationText
        {
            get { return ConvertToString(GetPlayerDuration()); }
        }

        public string[] Sources
        {
            get { return sources; }
            set
            {
                if (value == sources || (value != null) && value.SequenceEqual(sources)) return;

                sources = value;
                OnNotifyPropertyChanged("Sources");
                Refresh();
            }
        }

        public Song[] Songs
        {
            get { return songs; }
            set
            {
                if (value == songs || value == null || value.SequenceEqual(songs)) return;

                songs = value;

                OnNotifyPropertyChanged("Songs");
                OnNotifyPropertyChanged("CurrentShowSong");
            }
        }

        public ViewModel(MediaElement player)
        {
            this.player = player;

            isAllSongsShuffle = true;
            lowerSearchKey = searchKey = string.Empty;

            allSongsShuffled = new Song[0];
            songs = new Song[0];

            Sources = Environment.GetCommandLineArgs().Skip(1).ToArray();
        }

        public void Refresh()
        {
            allSongsShuffled = GetShuffledSongs(GetAllSongs()).ToArray();

            for (int i = 0; i < allSongsShuffled.Length; i++) allSongsShuffled[i].Index = i;

            UpdateSongs();

            if (!allSongsShuffled.Contains(CurrentPlaySong)) CurrentPlaySong = allSongsShuffled.FirstOrDefault();
        }

        private IEnumerable<Song> GetAllSongs()
        {
            try
            {

                IEnumerable<Song> allSongs = GetSourcePaths().SelectMany(GetSongs);
                IEnumerable<Song> notHiddenSongs = allSongs.Where(s => !s.IsHidden);

                return notHiddenSongs.ToList();
            }
            catch
            {
                return Enumerable.Empty<Song>();
            }
        }

        private IEnumerable<string> GetSourcePaths()
        {
            if (sources != null && sources.Any()) return sources;

            string defaultPath;

            try
            {
                defaultPath = File.ReadAllText(folderPathFileName);
            }
            catch
            {
                defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            }

            return Enumerable.Repeat(defaultPath, 1);
        }

        private IEnumerable<Song> GetSongs(string path)
        {
            if (File.Exists(path)) yield return new Song(path);

            if (Directory.Exists(path))
            {
                foreach (string file in Directory.GetFiles(path)) yield return new Song(file);
            }
        }

        private IEnumerable<Song> GetShuffledSongs(IEnumerable<Song> songs)
        {
            return songs.OrderBy(s => ran.Next()).ToList();
        }

        private void UpdateSongs()
        {
            Songs = GetSongs().ToArray();
        }

        private IEnumerable<Song> GetSongs()
        {
            if (!IsSearching && IsShuffle) return allSongsShuffled;
            if (!IsSearching && !IsShuffle) return GetOrderedSongs();
            if (IsSearching && !IsShuffle) return GetFilteredSongs();

            return GetFilteredSongs().OrderBy(s => ((IList)allSongsShuffled).IndexOf(s));
        }

        private IEnumerable<Song> GetFilteredSongs()
        {
            if (!IsSearching) return Enumerable.Empty<Song>();

            IEnumerable<Song> mixedTitle = allSongsShuffled.Where(CT).OrderBy(TI).ThenBy(s => s.Title).ThenBy(s => s.Artist);
            IEnumerable<Song> mixedArtist = allSongsShuffled.Where(CA).OrderBy(AI).ThenBy(s => s.Artist).ThenBy(s => s.Title);
            IEnumerable<Song> lowerTitle = allSongsShuffled.Where(CLT).OrderBy(LTI).ThenBy(s => s.Title).ThenBy(s => s.Artist);
            IEnumerable<Song> lowerArtist = allSongsShuffled.Where(CLA).OrderBy(LAI).ThenBy(s => s.Artist).ThenBy(s => s.Title);

            return mixedTitle.Concat(mixedArtist).Concat(lowerTitle).Concat(lowerArtist).Distinct();
        }

        #region Filtermethods
        private bool CT(Song song)
        {
            return song.Title.Contains(searchKey);
        }

        private bool CA(Song song)
        {
            return song.Artist.Contains(searchKey);
        }

        private bool CLT(Song song)
        {
            return song.Title.ToLower().Contains(lowerSearchKey);
        }

        private bool CLA(Song song)
        {
            return song.Artist.ToLower().Contains(lowerSearchKey);
        }

        private int TI(Song song)
        {
            return song.Title.IndexOf(searchKey);
        }

        private int AI(Song song)
        {
            return song.Artist.IndexOf(searchKey);
        }

        private int LTI(Song song)
        {
            return song.Title.ToLower().IndexOf(lowerSearchKey);
        }

        private int LAI(Song song)
        {
            return song.Artist.ToLower().IndexOf(lowerSearchKey);
        }
        #endregion

        private IEnumerable<Song> GetOrderedSongs()
        {
            return allSongsShuffled.OrderBy(s => s.Title).ThenBy(s => s.Artist);
        }

        public void SetNextSong()
        {
            SetNextSong(Songs);
        }

        private void SetNextSong(IList<Song> songs)
        {
            int index = songs.IndexOf(currentSong);

            if (index == -1 && !songs.Any()) return;

            index = (index + 1) % songs.Count;
            CurrentPlaySong = songs.ElementAtOrDefault(index);
        }

        public void SetPreviousSong()
        {
            SetPreviousSong(Songs);
        }

        public void SetPreviousSong(IList<Song> songs)
        {
            int index = songs.IndexOf(currentSong);

            if (index == -1)
            {
                if (!songs.Any()) return;
                index = 1;
            }

            index = (index + songs.Count - 1) % songs.Count;
            CurrentPlaySong = songs.ElementAtOrDefault(index);
        }

        public void UpdateCurrentPlaySongPosition()
        {
            try
            {
                TimeSpan position = player.Position;
                TimeSpan duration = GetPlayerDuration();

                sliderPercent = duration.TotalDays > 0 ? position.TotalDays / duration.TotalDays : 0;

                OnNotifyPropertyChanged("CurrentPlaySongPosition");
                OnNotifyPropertyChanged("CurrentPlaySongPositionText");
                OnNotifyPropertyChanged("CurrentPlaySongDurationText");
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, "ViewModel.UpdateCurrentPlaySongPosition()");
            }
        }

        private TimeSpan GetPlayerDuration()
        {
            return player.NaturalDuration.HasTimeSpan ? player.NaturalDuration.TimeSpan : player.Position;
        }

        private string ConvertToString(TimeSpan ts)
        {
            return ts.Hours > 0 ? ts.ToString("hh\\.mm\\.ss") : ts.ToString("mm\\.ss");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnNotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
