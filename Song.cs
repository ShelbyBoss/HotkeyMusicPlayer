using System;
using System.IO;

namespace HotkeyMusicPlayer
{
    struct Song
    {
        private const string seperator = " - ", unkownArtist = "Unkown";

        public int Index { get; set; }

        public bool IsHidden { get; private set; }

        public string Artist { get; private set; }

        public string Title { get; private set; }

        public Uri Uri { get; private set; }

        public Song(string path)
        {
            Index = -1;

            string fileName = Path.GetFileNameWithoutExtension(path);
            int index = fileName.IndexOf(seperator);

            if (index != -1)
            {
                Artist = fileName.Remove(index);
                Title = fileName.Remove(0, index + seperator.Length);
            }
            else
            {
                Artist = unkownArtist;
                Title = fileName;
            }

            IsHidden = (new FileInfo(path).Attributes & FileAttributes.Hidden) > 0;
            Uri = new Uri(path);
        }

        public override string ToString()
        {
            return Artist + seperator + Title;
        }

        public override bool Equals(object obj)
        {
            return obj is Song ? (Song)obj == this : false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Song song1, Song song2)
        {
            return song1.Uri?.OriginalString == song2.Uri?.OriginalString;
        }

        public static bool operator !=(Song song1, Song song2)
        {
            return !(song1 == song2);
        }
    }
}
