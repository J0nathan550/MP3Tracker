using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace MP3Tracker
{
    public partial class MP3Tracker_main : Window
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();
        DispatcherTimer updateTimer;
        private Random random = new Random();
        private Regex mp3Cleaner = new Regex("\\.mp3");

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
            dialog.Filter = "MP3 files (*.mp3)|*.mp3|Video files (*.mp4)|*.mp4|All files (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                AddMusicFile(dialog.FileName);
            }
        }

        private void OpenMusicFolderToPlay(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                foreach (var file in Directory.GetFiles(dialog.FileName))
                {
                    AddMusicPlaylistFile(file, dialog.FileName);
                }
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
                playlistButton[indexButtonSelected].Style = (Style)Resources["buttonDesignSelected"];
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

            playlistButton[indexButtonSelected].Style = (Style)Resources["buttonDesign"];

            image = null;

            MusicSlider.IsEnabled = false;

        }

        private int indexButtonSelected = 0;
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
                playlistButton[indexButtonSelected].Style = (Style)Resources["buttonDesign"];
                int index = playlist.IndexOf(currentMusic);
                index++;
                if (index >= playlist.Count)
                {
                    index = 0;
                }

                indexButtonSelected = index;
                playlistButton[indexButtonSelected].Style = (Style)Resources["buttonDesignSelected"];

                PlayMusic(playlist[index]);
            }
            else if (!isOnRepeat && MusicSlider.Value == MusicSlider.Maximum && playlist.Count > 1 && isRandomSelected)
            {
                playlistButton[indexButtonSelected].Style = (Style)Resources["buttonDesign"];

                int index = playlist.IndexOf(currentMusic);
                index = random.Next(0, playlist.Count);

                indexButtonSelected = index;
                playlistButton[indexButtonSelected].Style = (Style)Resources["buttonDesignSelected"];

                PlayMusic(playlist[index]);
            }
            else if (!isOnRepeat && MusicSlider.Value == MusicSlider.Maximum)
            {
                playlistButton[indexButtonSelected].Style = (Style)Resources["buttonDesign"];
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
            
            playlistButton[indexButtonSelected].Style = (Style)Resources["buttonDesign"];
            
            int index = playlist.IndexOf(currentMusic);
            index--;
            if (index >= playlist.Count || index < 0)
            {
                index = 0;
            }

            indexButtonSelected = index;
            playlistButton[indexButtonSelected].Style = (Style)Resources["buttonDesignSelected"];

            PlayMusic(playlist[index]);
            isClickedButtonToStart = false;
        }

        private void ButtonToEnd_Click(object sender, RoutedEventArgs e) // << 
        {
            if (!isPlayable)
            {
                return;
            }
            if (isOnRepeat)
            {
                mediaPlayer.Position = mediaPlayer.NaturalDuration.TimeSpan;
                return;
            }
            playlistButton[indexButtonSelected].Style = (Style)Resources["buttonDesign"];
           
            int index = playlist.IndexOf(currentMusic);
            index++;
            if (index >= playlist.Count || index < 0)
            {
                index = 0;
            }
            indexButtonSelected = index;
            playlistButton[indexButtonSelected].Style = (Style)Resources["buttonDesignSelected"];

            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                mediaPlayer.Position = mediaPlayer.NaturalDuration.TimeSpan;
            }
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
        private List<Button> playlistButton = new List<Button>();
        private FileInfo currentMusic = null;

        private void AddMusicFile(string file)
        {
            FileInfo info = new FileInfo(file);

            if (!info.Extension.ToLower().Equals(".mp3"))
            {
                return;
            }

            foreach (var item in playlist)
            {
                if (info.FullName == item.FullName)
                {
                    info = null;
                    return;
                }
            }

            playlist.Add(info);

            Button b = new Button();
            b.Tag = info;
            b.Width = 223;
            b.Margin = new Thickness(0, 7, 0, 5);
            b.Click += MusicSelected;
            b.Style = (Style)Resources["buttonDesign"];
            b.Content = mp3Cleaner.Replace(info.Name, "");


            treeView.Items.Add(b);
            playlistButton.Add(b);

            // made a code that will check if file already in player.


            if (currentMusic == null)
            {
                PlayMusic(info);
            }
        }

        private List<TreeViewItem> listTreeViewItems = new List<TreeViewItem>();
        private void AddMusicPlaylistFile(string file, string folderName)
        {
            FileInfo info = new FileInfo(file);

            if (!info.Extension.ToLower().Equals(".mp3"))
            {
                return;
            }

            foreach (var item in playlist)
            {
                if (info.FullName == item.FullName)
                {
                    info = null;
                    return;
                }
            }

            playlist.Add(info);

            Button b = new Button();
            b.Tag = info;
            b.Width = 223;
            b.Margin = new Thickness(0, 7, 0, 5);
            b.Click += MusicSelected;
            b.Style = (Style)Resources["buttonDesign"];
            b.Content = mp3Cleaner.Replace(info.Name, "");

            TreeViewItem treeViewItem = new TreeViewItem();

            treeViewItem.Header = folderName;
            treeViewItem.Style = (Style)Resources["treeViewDesign"];
            treeViewItem.Margin = new Thickness(0, 5, 0, 0);

            foreach (var item in listTreeViewItems)
            {
                if (item.Header == treeViewItem.Header)
                {
                    item.Items.Add(b);
                    playlistButton.Add(b);
                    if (currentMusic == null)
                    {
                        PlayMusic(info);
                    }
                    treeViewItem = null;
                    return;
                }
            }

            treeViewItem.Items.Add(b);
            treeView.Items.Add(treeViewItem);
            listTreeViewItems.Add(treeViewItem);

            playlistButton.Add(b);


            if (currentMusic == null)
            {
                PlayMusic(info);
            }
        }

        private void MusicSelected(object sender, RoutedEventArgs e)
        {
            FileInfo file = (FileInfo)(((Button)sender).Tag);
            
            playlistButton[indexButtonSelected].Style = (Style)Resources["buttonDesign"];
            int index = playlist.IndexOf(file);
            indexButtonSelected = index;
            playlistButton[indexButtonSelected].Style = (Style)Resources["buttonDesignSelected"];
            
            PlayMusic(file);
        }

        private void PlayMusic(FileInfo file)
        {
            mediaPlayer.Open(new Uri(file.FullName));
            MusicName.Content = mp3Cleaner.Replace(file.Name, "");
            currentMusic = file;
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                MusicSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            }

            playlistButton[indexButtonSelected].Style = (Style)Resources["buttonDesignSelected"];
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