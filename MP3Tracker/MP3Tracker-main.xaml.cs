using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace MP3Tracker
{
    public partial class MP3Tracker_main : Window
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();
        DispatcherTimer updateTimer;
        public MP3Tracker_main()
        {
            InitializeComponent();
            if (!isPlayable)
            {
                MusicSlider.IsEnabled = false;
            }
            updateTimer = new DispatcherTimer();
            updateTimer.Interval = TimeSpan.FromMilliseconds(1);
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
                mediaPlayer.Open(new Uri(dialog.FileName));
                MusicName.Content = $"{dialog.SafeFileName}";
                if (mediaPlayer.NaturalDuration.HasTimeSpan)
                {
                   EndTimeLabel.Content = mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                }
            }
        }

        private bool isPlayable = false;
        private void ButtonStart_Click(object sender, RoutedEventArgs e)
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
                
                return;
            }
            
            mediaPlayer.Pause();

            image.BeginInit();
            image.UriSource = new Uri("pack://siteoforigin:,,,/Images/playButton.png");
            image.EndInit();
            playIcon.Source = image;

            MusicSlider.IsEnabled = false;

        }

        private void MusicUpdate()
        {
            ProgressTimeLabel.Content = mediaPlayer.Position.ToString(@"mm\:ss");
            EndTimeLabel.Content = mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");

            MusicSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            MusicSlider.Value = mediaPlayer.Position.TotalSeconds;

            if (isOnRepeat && MusicSlider.Value == MusicSlider.Maximum)
            {
                mediaPlayer.Stop();
                mediaPlayer.Play();
            }
            else if (!isOnRepeat && MusicSlider.Value == MusicSlider.Maximum)
            {
                mediaPlayer.Stop();

                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri("pack://siteoforigin:,,,/Images/playButton.png");
                image.EndInit();
                playIcon.Source = image;

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
        }

        private void ButtonToStart_Click(object sender, RoutedEventArgs e) // >>
        {
            if (!isPlayable)
            {
                return;
            }
            mediaPlayer.Stop();
            mediaPlayer.Play();
        }

        private void ButtonToEnd_Click(object sender, RoutedEventArgs e) // << 
        {
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
        }

        private void MusicSlider_Move(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TimeSpan time = TimeSpan.FromSeconds(MusicSlider.Value);    
            mediaPlayer.Position = time;
        }
    }
}