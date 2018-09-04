using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace HotkeyMusicPlayer
{
    class IsCheckedToMediaState : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (MediaState)value == MediaState.Play;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? MediaState.Play : MediaState.Pause;
        }
    }
}
