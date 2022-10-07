using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO;
using System.Collections.Generic;
using System.Windows.Controls;

namespace MP3Tracker
{
    public partial class MP3Tracker_main : Window
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();
        DispatcherTimer updateTimer;
        private Random random = new Random();

        public MP3Tracker_main()
        {
            InitializeComponent();
            if (!isPlayable)
            {
                MusicSlider.IsEnabled = false;
            }
            updateTimer = new DispatcherTimer();
            updateTimer.Interval = TimeSpan.FromMilliseconds(100);
            updateTimer.Tick += Update;
            updateTimer.Start();

        }


        private void Update(object sender, EventArgs e)
        {
            VolumeCheckIcon();
            if (isPlayable)
            {
                MusicUpdate();
            }
        }

        private void OpenMusicToPlay(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "MP3 files (*.mp3)|*.mp3|Ogg files (*.ogg)|*.ogg|All files (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                AddMusicFile(dialog.FileName);
            }
        }

        private bool isPlayable = false;
        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            ButtonStart_Click();
        }
        private void ButtonStart_Click()
        {
            if (mediaPlayer.Source == null)
            {
                MessageBox.Show("Сперва выбери путь к песне!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            BitmapImage image = new BitmapImage();
            isPlayable = !isPlayable;

            if (isPlayable)
            {
                MusicSlider.IsEnabled = true;
                mediaPlayer.Play();
                image.BeginInit();
                image.UriSource = new Uri("pack://siteoforigin:,,,/Images/pauseButton.png");
                image.EndInit();
                playIcon.Source = image;
                image = null;
                if (mediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    MusicSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                }

                return;
            }
            mediaPlayer.Pause();

            image.BeginInit();
            image.UriSource = new Uri("pack://siteoforigin:,,,/Images/playButton.png");
            image.EndInit();
            playIcon.Source = image;

            image = null;

            MusicSlider.IsEnabled = false;

        }

        private void MusicUpdate()
        {
            ProgressTimeLabel.Content = mediaPlayer.Position.ToString(@"mm\:ss");
            MusicSlider.Value = mediaPlayer.Position.TotalSeconds;
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                MusicSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                EndTimeLabel.Content = mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
            }

            if (isOnRepeat && MusicSlider.Value == MusicSlider.Maximum)
            {
                mediaPlayer.Stop();
                mediaPlayer.Play();
            }
            else if (!isOnRepeat && MusicSlider.Value == MusicSlider.Maximum && playlist.Count > 1 && !isRandomSelected)
            {
                int index = playlist.IndexOf(currentMusic);
                index++;
                if (index >= playlist.Count)
                {
                    index = 0;
                }
                PlayMusic(playlist[index]);
            }
            else if (!isOnRepeat && MusicSlider.Value == MusicSlider.Maximum && playlist.Count > 1 && isRandomSelected)
            {
                int index = playlist.IndexOf(currentMusic);
                index = random.Next(0, playlist.Count);
                PlayMusic(playlist[index]);
            }
            else if (!isOnRepeat && MusicSlider.Value == MusicSlider.Maximum)
            {
                mediaPlayer.Stop();
                currentMusic = null;
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri("pack://siteoforigin:,,,/Images/playButton.png");
                image.EndInit();
                playIcon.Source = image;

                image = null;
                isPlayable = false;
            }

        }

        private bool isOnRepeat = false;
        private void ButtonRepeat_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage image = new BitmapImage();
            isOnRepeat = !isOnRepeat;
            if (isOnRepeat)
            {
                image.BeginInit();
                image.UriSource = new Uri("pack://siteoforigin:,,,/Images/repeat-button-active.png");
                image.EndInit();
                buttonRepeatIcon.Source = image;
                return;
            }
            image.BeginInit();
            image.UriSource = new Uri("pack://siteoforigin:,,,/Images/repeat-button-deactive.png");
            image.EndInit();
            buttonRepeatIcon.Source = image;
            image = null;
        }

        private bool isClickedButtonToStart;
        private void ButtonToStart_Click(object sender, RoutedEventArgs e) // >>
        {
            if (!isPlayable)
            {
                return;
            }
            if (!isClickedButtonToStart)
            {
                mediaPlayer.Stop();
                mediaPlayer.Play();
                isClickedButtonToStart = true;
                return;
            }
            int index = playlist.IndexOf(currentMusic);
            index--;
            if (index >= playlist.Count || index < 0)
            {
                index = 0;
            }
            PlayMusic(playlist[index]);
            isClickedButtonToStart = false;
        }

        private void ButtonToEnd_Click(object sender, RoutedEventArgs e) // << 
        {
            if (!isPlayable)
            {
                return;
            }
            mediaPlayer.Position = mediaPlayer.NaturalDuration.TimeSpan;
        }

        private void VolumeChange_Move(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = VolumeSlider.Value;
        }

        private void VolumeCheckIcon()
        {
            BitmapImage image = new BitmapImage();
            if (VolumeSlider.Value == 0f)
            {
                image.BeginInit();
                image.UriSource = new Uri("pack://siteoforigin:,,,/Images/volume-muted-icon.png");
                image.EndInit();
                volumeIcon.Source = image;
            }
            else if (VolumeSlider.Value >= 0.2f && VolumeSlider.Value < 0.5f)
            {
                image.BeginInit();
                image.UriSource = new Uri("pack://siteoforigin:,,,/Images/volume-min-icon.png");
                image.EndInit();
                volumeIcon.Source = image;
            }
            else if (VolumeSlider.Value >= 0.5f && VolumeSlider.Value < 0.8f)
            {
                image.BeginInit();
                image.UriSource = new Uri("pack://siteoforigin:,,,/Images/volume-med-icon.png");
                image.EndInit();
                volumeIcon.Source = image;
            }
            else if (VolumeSlider.Value >= 0.8f)
            {
                image.BeginInit();
                image.UriSource = new Uri("pack://siteoforigin:,,,/Images/volume-big-icon.png");
                image.EndInit();
                volumeIcon.Source = image;
            }
            image = null;
        }

        private void MusicSlider_Move(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TimeSpan time = TimeSpan.FromSeconds(MusicSlider.Value);
            mediaPlayer.Position = time;
        }

        private void MusicDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
            {
                if (Directory.Exists(file))
                {
                    foreach (var path in Directory.EnumerateFiles(file))
                    {
                        AddMusicFile(path);
                    }
                }
                else
                {
                    AddMusicFile(file);
                }
            }
        }

        private List<FileInfo> playlist = new List<FileInfo>();
        private FileInfo currentMusic = null;

        private void AddMusicFile(string file)
        {
            FileInfo info = new FileInfo(file);

            if (!info.Extension.ToLower().Equals(".mp3"))
            {
                return;
            }


            playlist.Add(info);

            Button b = new Button();
            b.Tag = info;
            b.Click += MusicSelected;
            b.Content = info.Name;

            Playlist.Children.Add(b);

            // made a code that will check if file already in player.

            if (currentMusic == null)
            {
                PlayMusic(info);
            }
        }

        private void MusicSelected(object sender, RoutedEventArgs e)
        {
            FileInfo file = (FileInfo)(((Button)sender).Tag);
            PlayMusic(file);
            ButtonStart_Click();
        }

        private void PlayMusic(FileInfo file)
        {
            mediaPlayer.Open(new Uri(file.FullName));
            MusicName.Content = file.Name;
            currentMusic = file;
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                MusicSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            }
            isPlayable = false;
            ButtonStart_Click();
        }

        private bool isRandomSelected = false;
        private void RandomSong_Playlist_Clicked(object sender, RoutedEventArgs e)
        {
            if (!isPlayable)
            {
                return;
            }
            isRandomSelected = !isRandomSelected;
            BitmapImage image = new BitmapImage();
            if (isRandomSelected)
            {
                image.BeginInit();
                image.UriSource = new Uri("pack://siteoforigin:,,,/Images/randomShuffleIcon-active.png");
                image.EndInit();
                ShuffleIcon.Source = image;
                image = null;
                return;
            }
            image.BeginInit();
            image.UriSource = new Uri("pack://siteoforigin:,,,/Images/randomShuffleIcon.png");
            image.EndInit();
            ShuffleIcon.Source = image;
            image = null;
        }
    }
}