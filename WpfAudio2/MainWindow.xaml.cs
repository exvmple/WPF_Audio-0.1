using System.Collections.Generic;
//using System.Windows.Shapes;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WMPLib;
using TagLib;
using System.Windows.Media.Imaging;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Resources;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls.Primitives;

namespace WpfAudio2
{
    public class Cover
    {
        string filename;
        string dir;

        public Cover(string filename, string dir)
        {
            this.filename = filename;
            this.dir = dir;
        }

        public string Filename { get => filename; set => filename = value; }
        public string Dir { get => dir; set => dir = value; }
    }
    public class Artist
    {
        string name;
        public Artist(string name)
        {
            this.name = name;
        }

        public string Name { get => name; set => name = value; }
    }

    public class Album
    {
        string title;
        Artist artist;
        Cover cover;
        List<Song> songs = new List<Song>();

        public Album(string title, Artist artist, Cover cover)
        {
            this.title = title;
            this.artist = artist;
            this.cover = cover;
        }

        public string Title { get => title; set => title = value; }
        internal Artist Artist { get => artist; set => artist = value; }
        internal List<Song> Songs { get => songs; set => songs = value; }
        internal Cover Cover { get => cover; set => cover = value; }
    }
    public class Song
    {
        string duration;
        Artist artist;
        Album album;
        string title;
        string directory;

        public Song(Artist artist, Album album, string title, string dir, string time)
        {
            this.artist = artist;
            this.album = album;
            this.title = title;
            directory = dir;
            Duration = time;
        }

        public string Duration { get => duration; set => duration = value; }
        public string Title { get => title; set => title = value; }
        public string Directory { get => directory; set => directory = value; }
        public Album Album { get => album; set => album = value; }
        public Artist Artist { get => artist; set => artist = value; }
    
        public string Whole { get => this.title + " - " + artist.Name; }
    }

    class Playlist
    {
        string title;
        List<Song> playlist = new List<Song>();

        public Playlist(string title)
        {
            Title = title;
        }

        public string Title { get => title; set => title = value; }
        public List<Song> Songs { get => playlist; set => playlist = value; }
    }

    public class ProgramData
    {
        List<Song> songs = new List<Song>();
        List<Artist> artists = new List<Artist>();
        List<Album> albums = new List<Album>();
        List<Playlist> playlists = new List<Playlist>();

        internal List<Song> Songs { get => songs; set => songs = value; }
        internal List<Album> Albums { get => albums; set => albums = value; }
        internal List<Playlist> Playlists { get => playlists; set => playlists = value; }

        public void GetSongs(string dir)
        {
            string[] musicFiles = Directory.GetFiles(dir, "*.mp3");
            string[] covers = Directory.GetFiles(dir, "*over.jpg");
            foreach (string musicFile in musicFiles)
            {
                var mp3File = TagLib.File.Create(Path.Combine(dir, musicFile));

                Artist tempArtist;
                Album tempAlbum;
                string tempTitle = mp3File.Tag.Title;
                string tempDuration = mp3File.Properties.Duration.ToString("mm\\:ss");
                string tempDir = Path.Combine(dir, musicFile);


                Artist tempSongSearch = artists.Find(x => x.Name == mp3File.Tag.Performers[0]);
                if (tempSongSearch == null)
                {
                    tempArtist = new Artist(mp3File.Tag.Performers[0]);
                    artists.Add(tempArtist);
                }
                else
                {
                    tempArtist = tempSongSearch; 
                }

                Album tempAlbumSearch = albums.Find(x => x.Title == mp3File.Tag.Album && tempArtist.Name == x.Artist.Name);

                if(tempAlbumSearch == null)
                {
                   tempAlbum = new Album(mp3File.Tag.Album, tempArtist, new Cover(covers[0], dir));
                    albums.Add(tempAlbum);
                }
                else
                {
                    tempAlbum = tempAlbumSearch;
                }
               
                Song temp = new Song(tempArtist, tempAlbum, tempTitle, tempDir, tempDuration);
                if (!(Songs.Any(x => x.Title == temp.Title && x.Artist == temp.Artist)))
                {
                    
                    Songs.Add(temp);
                    tempAlbum.Songs.Add(temp);
                    
                }
            }
        }

    }

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool albumsStatus = false;
        bool playlistStatus = false;
  
        int temp = 0;
        string defdir = @"D:\C#\repos\Tragic City";
        ProgramData data = new ProgramData();
        Song currentSong;
        WMPLib.WindowsMediaPlayer player = new WMPLib.WindowsMediaPlayer();
        
    
        public MainWindow()
        {
            InitializeComponent();
            data.GetSongs(defdir);
            fillBoxes();
            listBoxSongs.SelectedIndex = 0;


            DoWorkAsyncInfiniteLoop();

        }



        void fillBoxes()
        {
            listBoxSongs.ItemsSource = data.Songs;
            listBoxSongs.DisplayMemberPath = "Whole";


            listBoxAlbums.ItemsSource = data.Albums;
            listBoxAlbums.DisplayMemberPath = "Title";

            listBoxPlaylists.ItemsSource = data.Playlists;
            listBoxPlaylists.DisplayMemberPath = "Title";

        }
        

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

           if(!TabAlbums.IsSelected)
            {
                buttonAlbBack.Visibility = Visibility.Hidden;
                
                if(TabSongs.IsSelected)
                {
                    addToPlaylist.Items.Clear();
                    foreach(Playlist play in data.Playlists)
                    {
                        
                        MenuItem mine = new MenuItem();
                        mine.Header = play.Title;

                        if(addToPlaylist.Items.OfType<MenuItem>().Where(i => i.Header == mine.Header).FirstOrDefault()==null)
                        {
                            mine.Click += mine_Click;
                            addToPlaylist.Items.Add(mine);
                        }
                    }

                    if(addToPlaylist.Items.Count==0)
                    {
                        MenuItem mine = new MenuItem();
                        mine.Header = "brak";
                        mine.IsEnabled = false;
                        addToPlaylist.Items.Add(mine);
                    }
                }

            }
           else
            {
                if(albumsStatus)
                {
                    buttonAlbBack.Visibility = Visibility.Visible;
                }

                addToPlaylistfromAlb.Items.Clear();
                foreach (Playlist play in data.Playlists)
                {

                    MenuItem mineAlb = new MenuItem();
                    mineAlb.Header = play.Title;

                    if (addToPlaylistfromAlb.Items.OfType<MenuItem>().Where(i => i.Header == mineAlb.Header).FirstOrDefault() == null)
                    {
                        mineAlb.Click += mineAlb_Click;
                        addToPlaylistfromAlb.Items.Add(mineAlb);
                    }
                }

                if (addToPlaylist.Items.Count == 0)
                {
                    MenuItem mineAlb = new MenuItem();
                    mineAlb.Header = "brak";
                    mineAlb.IsEnabled = false;
                    addToPlaylistfromAlb.Items.Add(mineAlb);
                }

            }
            if (TabPlayer.IsSelected)
            {
                if(currentSong!=null)
                {
                    tabPlayerPic.Source = new BitmapImage(new System.Uri(currentSong.Album.Cover.Filename));
                    textBlockSongInfo.Text = currentSong.Title + " \n" + currentSong.Artist.Name + " - " + currentSong.Album.Title;

                    TimeSpan dur = TimeSpan.ParseExact(currentSong.Duration, "mm\\:ss", System.Globalization.CultureInfo.InvariantCulture);
                    TimeSpan cur = TimeSpan.FromSeconds(player.controls.currentPosition);


                    textBlockDuration.Text = string.Format("{0:mm\\:ss}", dur);
                }
                else
                {
                    tabPlayerPic.Source = new BitmapImage(new Uri(Path.Combine(Environment.CurrentDirectory,@"temp\no.png")));
                    textBlockSongInfo.Text = "none";
                    textBlockDuration.Text = "00:00";
                    textBlockCurrentPos.Text = "00:00";
                }
                
            }
            
        }

        private void mineAlb_Click(object sender, RoutedEventArgs e)
        {
            if (albumsStatus == false)
            {
                MenuItem m = (MenuItem)sender;

                Playlist playlist = data.Playlists.Find(x => x.Title == m.Header.ToString());

                Album mine = data.Albums[listBoxAlbums.SelectedIndex];

                foreach(Song song in mine.Songs)
                {
                    playlist.Songs.Add(song);
                }

            }
            else
            {
                MenuItem m = (MenuItem)sender;
                foreach (Song mine in listBoxAlbums.SelectedItems)
                {
                    Song song = mine;

                    Playlist selected = data.Playlists.Find(x => x.Title == m.Header.ToString());

                    if (selected != null)
                    {
                        selected.Songs.Add(song);
                    }
                }
            }
        }

        private void mine_Click(object sender, RoutedEventArgs e)
        {
            MenuItem m = (MenuItem)sender;
            foreach (Song mine in listBoxSongs.SelectedItems)
            {
                Song song = mine;

                Playlist selected = data.Playlists.Find(x => x.Title == m.Header.ToString());

                if (selected != null)
                {
                    selected.Songs.Add(song);
                }
            } 
        }

        private void buttonPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if(player.playState == WMPPlayState.wmppsUndefined)
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

        private void listBoxSongs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void listBoxSongs_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (player.URL != data.Songs[listBoxSongs.SelectedIndex].Directory)
            {
                currentSong = data.Songs[listBoxSongs.SelectedIndex];
                player.URL = data.Songs[listBoxSongs.SelectedIndex].Directory;
                player.controls.play();
                buttonPlayPause.Content = "Pause";
            }
        }

        private void listBoxAlbums_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void listBoxAlbums_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(listBoxAlbums.SelectedIndex > -1 && listBoxAlbums.SelectedIndex < listBoxAlbums.Items.Count)
            {
                if (albumsStatus == false)
                {
                    temp = listBoxAlbums.SelectedIndex;

                    listBoxAlbums.ItemsSource = null;

                    listBoxAlbums.ItemsSource = data.Albums[temp].Songs;
                    listBoxAlbums.DisplayMemberPath = "Title";

                    albumsStatus = true;

                    buttonAlbBack.Visibility = Visibility.Visible;

                }
                else
                {
                    currentSong = data.Albums[temp].Songs[listBoxAlbums.SelectedIndex];
                    player.URL = data.Albums[temp].Songs[listBoxAlbums.SelectedIndex].Directory;
                    player.controls.play();
                    buttonPlayPause.Content = "Pause";
                }
            }
                

        }

        private void buttonAlbBack_Click(object sender, RoutedEventArgs e)
        {
            listBoxAlbums.ItemsSource = null;
            listBoxAlbums.ItemsSource = data.Albums;
            listBoxAlbums.DisplayMemberPath = "Title";
            albumsStatus = false;

            buttonAlbBack.Visibility = Visibility.Hidden;
        }

        private void listBoxSongs_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            
        }
        

        private void listBoxSongsAddtoPlaylist(object sender, RoutedEventArgs e)
        {
            
        }

        private void buttonAddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            WindowAddPlaylist temp = new WindowAddPlaylist();
            temp.Show();
            temp.Owner = this;

            temp.WindowClose += Temp_WindowClose;
        }

        private void Temp_WindowClose(WindowAddPlaylist a)
        {
            if(a.textBox.Text !="")
            {
                string temp = a.textBox.Text;

                Playlist mine = new Playlist(temp);

                data.Playlists.Add(mine);

                listBoxPlaylists.Items.Refresh();

            }
            else
            {
                MessageBox.Show("Selected name is empty");
            }
            a.Close();
        }

        private void listBoxPlaylists_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void listBoxPlaylists_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (listBoxPlaylists.SelectedIndex > -1 && listBoxPlaylists.SelectedIndex < listBoxPlaylists.Items.Count)
            {
                if (playlistStatus == false)
                {
                    temp = listBoxPlaylists.SelectedIndex;

                    listBoxPlaylists.ItemsSource = null;

                    listBoxPlaylists.ItemsSource = data.Playlists[temp].Songs;
                    listBoxPlaylists.DisplayMemberPath = "Title";

                    playlistStatus = true;

                    buttonPlaylistBack.Visibility = Visibility.Visible;
                    buttonAddPlaylist.Visibility = Visibility.Hidden;
                    buttonRemovePlaylist.Visibility = Visibility.Hidden;

                    buttonPlaylistSongAdd.Visibility = Visibility.Visible;
                    buttonPlaylistSongRemove.Visibility = Visibility.Visible;

                }
                else
                {
                    currentSong = data.Playlists[temp].Songs[listBoxPlaylists.SelectedIndex];
                    player.URL = data.Playlists[temp].Songs[listBoxPlaylists.SelectedIndex].Directory;
                    player.controls.play();
                    buttonPlayPause.Content = "Pause";
                }
            }
        }

        private void buttonPlaylistBack_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void buttonPlaylistBack_Click_1(object sender, RoutedEventArgs e)
        {
            listBoxPlaylists.ItemsSource = null;
            listBoxPlaylists.ItemsSource = data.Playlists;
            listBoxPlaylists.DisplayMemberPath = "Title";
            
            playlistStatus = false;

            buttonPlaylistBack.Visibility = Visibility.Hidden;
            buttonAddPlaylist.Visibility = Visibility.Visible;
            buttonRemovePlaylist.Visibility = Visibility.Visible;

            buttonPlaylistSongAdd.Visibility = Visibility.Hidden;
            buttonPlaylistSongRemove.Visibility = Visibility.Hidden;
        }

        private void buttonRemovePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if(listBoxPlaylists.SelectedIndex>-1 && listBoxPlaylists.SelectedIndex<listBoxPlaylists.Items.Count)
            {
                if (data.Playlists[listBoxPlaylists.SelectedIndex] != null)
                {
                    data.Playlists.Remove(data.Playlists[listBoxPlaylists.SelectedIndex]);

                    listBoxPlaylists.Items.Refresh();
                }
            }
        }

        private void addToPlaylist_Selected(object sender, RoutedEventArgs e)
        {
            
        }

        private void addToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            

            
        }

        private void sliderPosition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        public void update_Slider(Song currentSong)
        {
            if(currentSong != null)
            {
                TimeSpan dur = TimeSpan.ParseExact(currentSong.Duration, "mm\\:ss", System.Globalization.CultureInfo.InvariantCulture);
                TimeSpan cur = TimeSpan.FromSeconds(player.controls.currentPosition);
                
                sliderPosition.Value= (cur.TotalSeconds / dur.TotalSeconds) * 100;
                textBlockCurrentPos.Text = string.Format("{0:mm\\:ss}", cur);

            }
            
        }

        private async Task DoWorkAsyncInfiniteLoop()
        {
            while (true)
            {
                if(player.playState == WMPPlayState.wmppsPlaying)
                {
                update_Slider(currentSong);
                }
                
                await Task.Delay(300);
            }
        }

        public class SliderIgnoreDelta : Slider
        {
            protected override void OnThumbDragDelta(DragDeltaEventArgs e)
            {
                // Do nothing
            }
        }

        private void sliderPosition_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            player.controls.currentPosition = (sliderPosition.Value / 100) * TimeSpan.ParseExact(currentSong.Duration, "mm\\:ss", System.Globalization.CultureInfo.InvariantCulture).TotalSeconds;
            TimeSpan cur = TimeSpan.FromSeconds(player.controls.currentPosition);
            textBlockCurrentPos.Text = string.Format("{0:mm\\:ss}", cur);
        }

        private void addToPlaylistfromAlb_Click(object sender, RoutedEventArgs e)
        {

        }
    }

   
}
