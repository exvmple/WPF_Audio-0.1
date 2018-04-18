using System;
using System.Collections.Generic;
using System.Linq;
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
using WMPLib;

namespace WpfAudio2
{
    /// <summary>
    /// Логика взаимодействия для WindowAlb.xaml
    /// </summary>
    public partial class WindowAlb : Window
    {
        WMPLib.WindowsMediaPlayer player;
        Album album;
        ListBox listIn = new ListBox();
        public WindowAlb()
        {
            
            InitializeComponent();
            
        }

        public WindowAlb(ListBox list ,WMPLib.WindowsMediaPlayer player, Album x)
        {
            
            InitializeComponent();
            textBlock.Text = x.Title;
            listBoxSelectedAlbum.ItemsSource = x.Songs;
            listBoxSelectedAlbum.DisplayMemberPath = "Title";
            
            this.player = player;
            this.album = x;
            this.listIn = list;

        }

        private void buttonPlayPause_Click(object sender, RoutedEventArgs e) 
        {
            if (player.playState == WMPPlayState.wmppsUndefined)
            {
                player.controls.play();
                buttonPlayPause.Content = "Pause";
            }
            else
            {
                if (player.playState == WMPPlayState.wmppsPlaying)
                {
                    player.controls.pause();
                    buttonPlayPause.Content = "Play";
                }
                else
                {
                    if (player.playState == WMPPlayState.wmppsPaused)
                    {
                        player.controls.play();
                        buttonPlayPause.Content = "Pause";
                    }
                }

            }
        }

        private void buttonPlayPause_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void listBoxSelectedAlbum_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

                player.URL = album.Songs[listBoxSelectedAlbum.SelectedIndex].Directory;
                player.controls.play();
                buttonPlayPause.Content = "Pause";
                
        }

        private void buttonAlbumBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            this.Closed += WindowAlb_Closed;
        }

        private void buttonAlbumBack_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
            this.Closed += WindowAlb_Closed;
        }

        private void WindowAlb_Closed(object sender, EventArgs e)
        {
            this.Owner = null;
        }
    }
}
