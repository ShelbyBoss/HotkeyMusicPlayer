using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HotkeyMusicPlayer
{
    class MaxWidthConverter : IValueConverter
    {
        private const double targetRange = 0.1;

        private bool isReset = false;
        private double currentMaxWidth = double.PositiveInfinity;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double width = (double)value;
            MaxValueProperties props = (MaxValueProperties)parameter;
            double target = props.TargetValueOfSource;
            double minWidth = props.MinTargetValue;
            bool wasReset = isReset;

            isReset = false;

            if (Math.Abs(width - target) / target <= targetRange) return currentMaxWidth;

            if (width < target || double.IsInfinity(currentMaxWidth))
            {
                if (wasReset) return currentMaxWidth;
                else return currentMaxWidth = minWidth;
            }

            currentMaxWidth = currentMaxWidth - target + width;
            if (currentMaxWidth < minWidth) currentMaxWidth = minWidth;

            return currentMaxWidth;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
