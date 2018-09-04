using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HotkeyMusicPlayer
{
    class MaxValueProperties : DependencyObject
    {

        public static readonly DependencyProperty MinTargetValueProperty =
            DependencyProperty.Register("MinTargetValue", typeof(double), typeof(MaxValueProperties),
                new PropertyMetadata(0.0, new PropertyChangedCallback(OnMinTargetValuePropertyChanged)));

        private static void OnMinTargetValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var s = (MaxValueProperties)sender;
            var value = (double)e.NewValue;
        }

        public static readonly DependencyProperty TargetValueOfSourceProperty =
            DependencyProperty.Register("TargetValueOfSource", typeof(double), typeof(MaxValueProperties),
                new PropertyMetadata(0.0, new PropertyChangedCallback(OnTargetValueOfSourcePropertyChanged)));

        private static void OnTargetValueOfSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var s = (MaxValueProperties)sender;
            var value = (double)e.NewValue;
        }

        public double MinTargetValue
        {
            get { return (double)GetValue(MinTargetValueProperty); }
            set { SetValue(MinTargetValueProperty, value); }
        }

        public double TargetValueOfSource
        {
            get { return (double)GetValue(TargetValueOfSourceProperty); }
            set { SetValue(TargetValueOfSourceProperty, value); }
        }
    }
}
