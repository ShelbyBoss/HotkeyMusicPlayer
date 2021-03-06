﻿using StdOttFramework.Hotkey;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace HotkeyMusicPlayer
{
    public partial class MainWindow : Window
    {
        private const string hotKeyFileName = "hotkeys.txt",
            previousKey = "Previous=", playPauseKey = "PlayPause=", nextKey = "Next=";

        private HotKey[] hotKeys;
        private DispatcherTimer timer;
        private ViewModel viewModel;
        private WidthService widthService;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = viewModel = new ViewModel(media);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(20);
            timer.Tick += Timer_Tick;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentPlaySong")
            {
                widthService.Reset();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string directory = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                string path = Path.Combine(directory, hotKeyFileName);
                string[] hotKeyLines = File.ReadAllLines(path);

                hotKeys = HotKey.GetRegisteredHotKeys(hotKeyLines, GetHotKeySources()).ToArray();
            }
            catch (Exception exc)
            {
                string message = string.Format("{0} while loading main window:\n{1}", exc.GetType().Name, exc.Message);
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            TogglePlayPause();

            widthService = new WidthService(tblTitle, tblArtist, cdSong, cdSlider);
            widthService.Reset();

            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private IEnumerable<HotKeySource> GetHotKeySources()
        {
            yield return new HotKeySource(previousKey, Previous_Pressed);
            yield return new HotKeySource(playPauseKey, PlayPause_Pressed);
            yield return new HotKeySource(nextKey, Next_Pressed);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            viewModel.UpdateCurrentPlaySongPosition();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                viewModel.Sources = (string[])e.Data.GetData(DataFormats.FileDrop);
            }
        }

        private void TblSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ScollToCurrentShowSong();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Refresh();
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            Previous();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            timer.Start();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            Next();
        }

        private void Media_MediaOpened(object sender, RoutedEventArgs e)
        {
            viewModel.UpdateCurrentPlaySongPosition();
        }

        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            Next();
        }

        private void Previous_Pressed(object sender, KeyPressedEventArgs e) => Previous();

        private void Previous()
        {
            viewModel.SetPreviousSong();

            ScollToCurrentShowSong();
        }

        private void PlayPause_Pressed(object sender, KeyPressedEventArgs e) => TogglePlayPause();

        private void TogglePlayPause()
        {
            media.LoadedBehavior = media.LoadedBehavior == MediaState.Play ? MediaState.Pause : MediaState.Play;
        }

        private void Next_Pressed(object sender, KeyPressedEventArgs e) => Next();

        private void Next()
        {
            viewModel.SetNextSong();

            ScollToCurrentShowSong();
        }

        private void TbxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (viewModel.Songs.Any())
                    {
                        viewModel.CurrentPlaySong = viewModel.Songs.First();
                        media.LoadedBehavior = MediaState.Play;
                    }
                    break;

                case Key.Escape:
                    viewModel.SearchKey = string.Empty;
                    break;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3) tbxSearch.Focus();
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            widthService?.Update();
        }

        private void ScollToCurrentShowSong()
        {
            if (viewModel.CurrentShowSong != null) lbxSongs.ScrollIntoView(viewModel.CurrentShowSong);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            foreach (HotKey hotKey in hotKeys ?? Enumerable.Empty<HotKey>()) hotKey?.Dispose();
        }

        private void StackPanel_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            TimeSpan position = media.Position;
            bool hasDuration = media.NaturalDuration.HasTimeSpan;
            TimeSpan duration = hasDuration ? media.NaturalDuration.TimeSpan : TimeSpan.Zero;
            bool timerIsEnabled = timer.IsEnabled;

            string format = "Position: {0}\nHasDuration: {1}\nDuration: {2}\nTimerIsEnabled: {3}";
            string message = string.Format(format, position, hasDuration, duration, timerIsEnabled);
            MessageBox.Show(message);
        }
    }
}
