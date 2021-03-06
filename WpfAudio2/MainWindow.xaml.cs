﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WMPLib;
using System.Drawing;
using System.Windows.Media.Imaging;
using System;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Timers;
using System.Windows.Media;
using System.Web;
using System.Net;
using System.Windows.Media.Effects;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data;

namespace WpfAudio2
{
    [Serializable]
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

        public string Whole { get => System.IO.Path.Combine(dir,filename); }
    }
    [Serializable]
    public class Artist
    {
        string name;
        public Artist(string name)
        {
            this.name = name;
        }


        public string Name { get => name; set => name = value; }
    }
    [Serializable]
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
        public Artist Artist { get => artist; set => artist = value; }
        internal List<Song> Songs { get => songs; set => songs = value; }
        public Cover Cover { get => cover; set => cover = value; }

        public string Whole
        {
            get
            {
                if (this.artist.Name != "(unknown)")
                {
                    return this.Artist.Name + " - " + this.Title;
                }
                else
                {
                    return this.Title;
                }
            }
        }
    }
    [Serializable]
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

        public string Whole
        {
            get
            {
                if (this.artist.Name != "(unknown)")
                {
                    return this.Title + " - " + this.artist.Name;
                }
                else
                {
                    return this.Title;
                }
            }
        }
    }

    [Serializable]
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
        public int Count { get => this.playlist.Count; }
    }

    [Serializable]
    public class ProgramData
    {
        List<Song> songs = new List<Song>();
        List<Artist> artists = new List<Artist>();
        List<Album> albums = new List<Album>();
        List<Playlist> playlists = new List<Playlist>();
        bool unsCreated = false;

        static Cover blank = new Cover("no.png", System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\"));
        static Artist unknown = new Artist("(unknown)");
        Album untitled = new Album("(untitled)", unknown, blank);
        

        public List<Song> Songs { get { return songs; } set { songs = value; } }


        internal List<Album> Albums { get => albums; set => albums = value; }
        internal List<Playlist> Playlists { get => playlists; set => playlists = value; }

        public void GetSongs(string dir)
        {
            var musicFiles = Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
             .Where(s => s.EndsWith(".mp3") || s.EndsWith(".m4a"));

            foreach (string musicFile in musicFiles)
            {
                GetSingleSong(dir, musicFile);
            }
        }


        public int GetSingleSong(string dir, string musicFile)
        {
            TagLib.File mp3File = null;
            try
            {
                mp3File = TagLib.File.Create(System.IO.Path.Combine(dir, musicFile));
            }
            catch(TagLib.CorruptFileException)
            {
                //MessageBox.Show("Corrupt file " + musicFile);
                return -1;
            }
            

            Artist tempArtist;
            Album tempAlbum;
            string tempTitle = mp3File.Tag.Title;
            string tempDuration = mp3File.Properties.Duration.ToString("mm\\:ss");
            string tempDir = System.IO.Path.Combine(dir, musicFile);
            string tempFolder = tempDir.Substring(0, tempDir.LastIndexOf(@"\"));


            if (mp3File.Tag.Performers == null || mp3File.Tag.Title == null || mp3File.Tag.Album == null)
            {
                if (!unsCreated)
                {
                    artists.Add(unknown);
                    albums.Add(untitled);
                    unsCreated = true;
                }
                string title = tempDir.Substring(tempDir.LastIndexOf(@"\") + 1);
                title = title.Substring(0, title.LastIndexOf('.') - 1);
                Song temp = new Song(unknown, untitled, title, tempDir, tempDuration);
                if (!(Songs.Any(x => x.Title == temp.Title)))
                {
                    Songs.Add(temp);
                    untitled.Songs.Add(temp);
                    return 0;
                }
                return 1;
            }
            else
            {
                if(mp3File.Tag.Performers.Length>0)
                {
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
                }
                else
                {
                    tempArtist = unknown;
                }

                Album tempAlbumSearch = albums.Find(x => x.Title == mp3File.Tag.Album && tempArtist.Name == x.Artist.Name);
                if (tempAlbumSearch == null)
                {
                    string[] covers = Directory.GetFiles(tempFolder, "*over.jpg");

                    if (covers.Length > 0)
                    {
                        tempAlbum = new Album(mp3File.Tag.Album, tempArtist, new Cover(covers[0], dir));
                    }
                    else
                    {
                        tempAlbum = new Album(mp3File.Tag.Album, tempArtist, blank);
                    }
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
                    return 0;
                }
                return 1;
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
        int selectedPlaylist = -1;
        int ffstatus = -1;
        public ProgramData data = new ProgramData();
        public Song currentSong;
        Random random = new Random();
        List<Song> currentSource = new List<Song>();
        public delegate bool Predicate<in Song>(Song obj);
        WMPLib.WindowsMediaPlayer player = new WMPLib.WindowsMediaPlayer();
        private static System.Timers.Timer timer;
        private static System.Timers.Timer nTimer;


        public MainWindow()
        {
            InitializeComponent();
            if(File.Exists("data.dat"))
            {
                readVar();
            }
            else
            {
                MessageBox.Show("Please choose the folder with your media");
                addFilesDial();
            }
            
            this.DataContext = data;
            FillBoxes();
            listBoxSongs.SelectedIndex = 0;

            timer = new System.Timers.Timer
            {
                Interval = 500,

                AutoReset = true
            };

            timer.Elapsed += Timer_Elapsed;

            player.PlayStateChange += Player_PlayStateChange;

            DoWorkAsyncInfiniteLoop();

            

        }

        private void Player_PlayStateChange(int NewState)
        {
            if ((WMPPlayState)NewState == WMPPlayState.wmppsMediaEnded)
            {
                if (currentSong != null)
                {
                    if(checkRepeat.IsChecked == true)
                    {

                    }
                    else
                    {
                        int temp = currentSource.FindIndex(x => x.Title == currentSong.Title && x.Artist == currentSong.Artist);

                        if (temp + 1 < currentSource.Count)
                        {
                            currentSong = currentSource[temp + 1];

                            player.URL = currentSong.Directory;
                            player.controls.play();

                            UpdatePlayer(currentSong);

                        }
                        else
                        {
                            currentSong = null;
                        }
                    }
                }
                SetTimer();

                UpdatePlayer(currentSong);
            }
        }

        private void SetTimer()
        {
            nTimer = new System.Timers.Timer(200);
            nTimer.Elapsed += OnTimedEvent;
            nTimer.Enabled = true;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            nTimer.Stop();
            if (currentSong != null)
            {
                player.controls.play();
            }
        }


        void FillBoxes()
        {
            if(data.Songs.Count>0)
            {
                currentSong = data.Songs[0];
                currentSource = data.Songs;
            }
            

            listBoxSongs.ItemsSource = data.Songs;


            listBoxAlbums.ItemsSource = data.Albums;

            listBoxPlaylists.ItemsSource = data.Playlists;

            
            UpdatePlayer(currentSong);

        }


        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            comboBoxNetworks.Visibility = Visibility.Hidden;
            if (!TabPlaylists.IsSelected)
            {
                buttonPlaylistBack.Visibility = Visibility.Hidden;
            }
            else
            {
                if (playlistStatus)
                {
                    buttonPlaylistBack.Visibility = Visibility.Visible;
                }
            }
            if (!TabAlbums.IsSelected)
            {
                buttonAlbBack.Visibility = Visibility.Hidden;

                if (TabSongs.IsSelected)
                {
                    addToPlaylist.Items.Clear();
                    foreach (Playlist play in data.Playlists)
                    {
                        MenuItem mine = new MenuItem
                        {
                            Header = play.Title
                        };

                        if (addToPlaylist.Items.OfType<MenuItem>().Where(i => i.Header == mine.Header).FirstOrDefault() == null)
                        {
                            mine.Click += Mine_Click;
                            addToPlaylist.Items.Add(mine);
                            listBoxPlaylists.Items.Refresh();
                        }
                    }

                    if (addToPlaylist.Items.Count == 0)
                    {
                        MenuItem mine = new MenuItem
                        {
                            Header = "brak",
                            IsEnabled = false
                        };
                        addToPlaylist.Items.Add(mine);
                    }
                }

            }
            else
            {
                
                
                if (albumsStatus)
                {
                    buttonAlbBack.Visibility = Visibility.Visible;
                }

                addToPlaylistfromAlb.Items.Clear();
                foreach (Playlist play in data.Playlists)
                {

                    MenuItem mineAlb = new MenuItem
                    {
                        Header = play.Title
                    };

                    if (addToPlaylistfromAlb.Items.OfType<MenuItem>().Where(i => i.Header == mineAlb.Header).FirstOrDefault() == null)
                    {
                        mineAlb.Click += MineAlb_Click;
                        addToPlaylistfromAlb.Items.Add(mineAlb);
                    }
                }

                if (addToPlaylist.Items.Count == 0)
                {
                    MenuItem mineAlb = new MenuItem
                    {
                        Header = "brak",
                        IsEnabled = false
                    };
                    addToPlaylistfromAlb.Items.Add(mineAlb);
                }

            }
            if (TabPlayer.IsSelected)
            {
                UpdatePlayer(currentSong);

                comboBoxNetworks.Visibility = Visibility.Visible;
            }
            

        }

        private void MineAlb_Click(object sender, RoutedEventArgs e)
        {
            if (albumsStatus == false)
            {
                MenuItem m = (MenuItem)sender;

                Playlist playlist = data.Playlists.Find(x => x.Title == m.Header.ToString());

                Album mine = data.Albums[listBoxAlbums.SelectedIndex];

                foreach (Song song in mine.Songs)
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

        private void Mine_Click(object sender, RoutedEventArgs e)
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

        private void ButtonPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (player.playState == WMPPlayState.wmppsUndefined)
            {
                if(currentSong!=null)
                {
                    player.URL = currentSong.Directory;
                }
                if (player.URL != null)
                {
                    player.controls.play();
                    buttonPlayPause.Content = FindResource("Pause");
                }
            }
            else
            {
                if (player.playState == WMPPlayState.wmppsPlaying)
                {
                    player.controls.pause();
                    buttonPlayPause.Content = FindResource("Play");
                }
                else
                {
                    if (player.playState == WMPPlayState.wmppsPaused)
                    {
                        player.controls.play();
                        buttonPlayPause.Content = FindResource("Pause");
                    }
                }

            }

        }

        private void ListBoxSongs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ListBoxSongs_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (player.URL != data.Songs[listBoxSongs.SelectedIndex].Directory)
            {
                currentSong = data.Songs[listBoxSongs.SelectedIndex];
                currentSource = data.Songs;
                player.URL = data.Songs[listBoxSongs.SelectedIndex].Directory;
                player.controls.play();
                buttonPlayPause.Content = FindResource("Pause");
            }
        }
        

        private void ListBoxAlbums_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (listBoxAlbums.SelectedIndex > -1 && listBoxAlbums.SelectedIndex < listBoxAlbums.Items.Count)
            {
                if (albumsStatus == false)
                {

                    albumsStatus = true;
                    temp = listBoxAlbums.SelectedIndex;

                    listBoxAlbums.ItemsSource = null;

                    listBoxAlbums.ItemsSource = data.Albums[temp].Songs;
                    //listBoxAlbums.DisplayMemberPath = "Title";


                    buttonAlbBack.Visibility = Visibility.Visible;

                }
                else
                {
                    currentSong = data.Albums[temp].Songs[listBoxAlbums.SelectedIndex];
                    currentSource = data.Albums[temp].Songs;
                    player.URL = data.Albums[temp].Songs[listBoxAlbums.SelectedIndex].Directory;
                    player.controls.play();
                    buttonPlayPause.Content = FindResource("Pause");
                }
            }


        }

        private void ButtonAlbBack_Click(object sender, RoutedEventArgs e)
        {
            listBoxAlbums.ItemsSource = null;
            listBoxAlbums.ItemsSource = data.Albums;
            //listBoxAlbums.DisplayMemberPath = "Title";
            albumsStatus = false;

            buttonAlbBack.Visibility = Visibility.Hidden;
        }

        private void ListBoxSongs_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }


        private void ListBoxSongsAddtoPlaylist(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonAddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            WindowAddPlaylist temp = new WindowAddPlaylist();
            temp.Show();
            temp.Owner = this;
            this.Effect = new BlurEffect();
            this.IsEnabled = false;
            temp.WindowClose += Temp_WindowClose;
        }

        private void Temp_WindowClose(WindowAddPlaylist a)
        {
            this.IsEnabled = true;
            this.Effect = null;
            if (a.textBox.Text != "")
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

        private void ListBoxPlaylists_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void ListBoxPlaylists_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (listBoxPlaylists.SelectedIndex > -1 && listBoxPlaylists.SelectedIndex < listBoxPlaylists.Items.Count)
            {
                if (playlistStatus == false)
                {
                    temp = listBoxPlaylists.SelectedIndex;

                    listBoxPlaylists.ItemsSource = null;

                    listBoxPlaylists.ItemsSource = data.Playlists[temp].Songs;
                    //listBoxPlaylists.DisplayMemberPath = "Title";

                    selectedPlaylist = temp;

                    playlistStatus = true;

                    Invert(0);

                }
                else
                {
                    currentSong = data.Playlists[temp].Songs[listBoxPlaylists.SelectedIndex];
                    currentSource = data.Playlists[temp].Songs;
                    player.URL = data.Playlists[temp].Songs[listBoxPlaylists.SelectedIndex].Directory;
                    player.controls.play();
                    buttonPlayPause.Content = FindResource("Pause");
                }
            }
        }

        private void Invert(int status) // Controls the visibility of buttons in TabPlaylist
        {
            if (status == 0)
            {
                buttonPlaylistBack.Visibility = Visibility.Visible;
                buttonAddPlaylist.Visibility = Visibility.Hidden;
                buttonRemovePlaylist.Visibility = Visibility.Hidden;

                buttonPlaylistSongRemove.Visibility = Visibility.Visible;
            }
            else
            {
                if (status == 1)
                {
                    buttonPlaylistBack.Visibility = Visibility.Hidden;
                    buttonAddPlaylist.Visibility = Visibility.Visible;
                    buttonRemovePlaylist.Visibility = Visibility.Visible;

                    buttonPlaylistSongRemove.Visibility = Visibility.Hidden;
                }
            }
        }
        

        private void ButtonPlaylistBack_Click_1(object sender, RoutedEventArgs e)
        {
            listBoxPlaylists.ItemsSource = null;
            listBoxPlaylists.ItemsSource = data.Playlists;
            //listBoxPlaylists.DisplayMemberPath = "Title";

            playlistStatus = false;

            Invert(1);

            selectedPlaylist = -1;
        }

        private void ButtonRemovePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxPlaylists.SelectedIndex > -1 && listBoxPlaylists.SelectedIndex < listBoxPlaylists.Items.Count)
            {
                if (data.Playlists[listBoxPlaylists.SelectedIndex] != null)
                {
                    data.Playlists.Remove(data.Playlists[listBoxPlaylists.SelectedIndex]);

                    listBoxPlaylists.Items.Refresh();
                }
            }
        }

        private void AddToPlaylist_Click(object sender, RoutedEventArgs e)
        {



        }

        private void SliderPosition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        public void Update_Slider(Song currentSong)
        {
            if (currentSong != null)
            {
                TimeSpan dur = TimeSpan.ParseExact(currentSong.Duration, "mm\\:ss", System.Globalization.CultureInfo.InvariantCulture);
                TimeSpan cur = TimeSpan.FromSeconds(player.controls.currentPosition);

                sliderPosition.Value = (cur.TotalSeconds / dur.TotalSeconds) * 100;
                textBlockCurrentPos.Text = string.Format("{0:mm\\:ss}", cur);
            }

        }

        private async Task DoWorkAsyncInfiniteLoop()
        {
            while (true)
                await NewMethod();
        }

        private async Task NewMethod()
        {
            if (player.playState == WMPPlayState.wmppsPlaying)
            {
                Update_Slider(currentSong);
            }

            await Task.Delay(300);
        }

        public class SliderIgnoreDelta : Slider
        {
            protected override void OnThumbDragDelta(DragDeltaEventArgs e)
            {
                // Do nothing
            }
        }

        private void SliderPosition_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            player.controls.currentPosition = (sliderPosition.Value / 100) * TimeSpan.ParseExact(currentSong.Duration, "mm\\:ss", System.Globalization.CultureInfo.InvariantCulture).TotalSeconds;
            TimeSpan cur = TimeSpan.FromSeconds(player.controls.currentPosition);
            textBlockCurrentPos.Text = string.Format("{0:mm\\:ss}", cur);

            if (player.playState != WMPPlayState.wmppsPlaying)
            {
                player.controls.play();
            }
        }

        private void AddToPlaylistfromAlb_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonPlaylistSongRemove_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPlaylist > -1)
            {

                Song mine = (Song)listBoxPlaylists.SelectedItem;


                data.Playlists[selectedPlaylist].Songs.Remove(mine);

                listBoxPlaylists.Items.Refresh();
            }
            else
            {

            }
        }

        private BitmapImage ImageFromStream(Stream stream)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();
            return image;
        }


        private void UpdatePlayer(Song currentSong)
        {
            if (currentSong != null)
            {

                tabPlayerPic.Source = new BitmapImage(new Uri(currentSong.Album.Cover.Whole, UriKind.Absolute));


                if (currentSong.Artist.Name != "(unknown)" && currentSong.Album.Title!= "(untitled)")
                {
                    textBlockSongInfo.Text = currentSong.Artist.Name + " - " + currentSong.Album.Title;
                    textBlockSongTitle.Text = currentSong.Title;
                }
                else
                {
                    textBlockSongInfo.Text = currentSong.Title;
                    textBlockSongTitle.Text = "";
                }


                TimeSpan dur = TimeSpan.ParseExact(currentSong.Duration, "mm\\:ss", System.Globalization.CultureInfo.InvariantCulture);
                TimeSpan cur = TimeSpan.FromSeconds(player.controls.currentPosition);

                textBlockCurrentPos.Text = string.Format("{0:mm\\:ss}", cur);
                textBlockDuration.Text = string.Format("{0:mm\\:ss}", dur);
                
                blurBack.Source = new BitmapImage(new Uri(currentSong.Album.Cover.Whole, UriKind.Absolute));


            }
            else
            {
                tabPlayerPic.Source = new BitmapImage(new Uri("no.png", UriKind.Relative));
                textBlockSongInfo.Text = "none";
                textBlockDuration.Text = "00:00";
                textBlockCurrentPos.Text = "00:00";
            }

        }


        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            if (currentSong != null)
            {
                if (checkRepeat.IsChecked != true)
                {
                    int temp = currentSource.IndexOf(currentSong);
                    if (checkRandom.IsChecked != true)
                    {
                        if (temp + 1 < currentSource.Count)
                        {
                            currentSong = currentSource[temp + 1];
                        }
                        else
                        {
                            currentSong = currentSource[0];
                        }
                    }
                    else
                    {
                        int next = temp;
                        while (next == currentSource.IndexOf(currentSong))
                        {
                            next = random.Next(0, currentSource.Count);
                        }

                        currentSong = currentSource[next];

                    }

                }
                player.URL = currentSong.Directory;
                player.controls.play();
                buttonPlayPause.Content = FindResource("Pause");

                UpdatePlayer(currentSong);

            }
            else
            {
                if (currentSource != null)
                {
                    currentSong = currentSource[0];
                    player.URL = currentSong.Directory;
                    player.controls.play();
                    buttonPlayPause.Content = FindResource("Pause");

                    UpdatePlayer(currentSong);

                }
            }
        }

        private void ButtonPrev_Click(object sender, RoutedEventArgs e)
        {
            if (currentSong != null)
            {
                
                if (checkRepeat.IsChecked != true)
                {
                    int temp = currentSource.IndexOf(currentSong);
                    if (checkRandom.IsChecked != true)
                    {
                        if (temp - 1 > -1)
                        {
                            currentSong = currentSource[temp - 1];
                        }
                        else
                        {
                            currentSong = currentSource[currentSource.Count - 1];
                        }
                    }
                    else
                    {
                        if (currentSource.Count > 1)
                        {
                            int next = temp;
                            while (next == currentSource.IndexOf(currentSong))
                            {
                                next = random.Next(0, currentSource.Count - 1);
                            }
                            currentSong = currentSource[next];
                        }

                    }

                }
                player.URL = currentSong.Directory;
                player.controls.play();
                buttonPlayPause.Content = FindResource("Pause");

                UpdatePlayer(currentSong);

            }
        }

        private void ButtonPrev_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
           
            timer.Enabled = true;
            ffstatus = 0;
        }
        private void ButtonPrev_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            timer.Enabled = false;
           
        }

        private void ButtonNext_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            timer.Enabled = true;
            ffstatus = 1;
        }
        private void ButtonNext_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            timer.Enabled = false;
          
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (ffstatus == 0)
            {
                if (player.controls.currentPosition - 5 > 0)
                {
                    player.controls.currentPosition -= 5;
                }
                else
                {
                    player.controls.currentPosition = 0;
                }
            }
            else
            {
                if (ffstatus == 1)
                {
                    if (player.controls.currentPosition + 5 < player.currentMedia.duration)
                    {
                        player.controls.currentPosition += 5;
                    }
                }
                else
                {

                }
            }

        }

        private void ComboInstagram_Selected(object sender, RoutedEventArgs e)
        {
            if(currentSong!=null)
            {
              
            }
        }
        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://clients3.google.com/generate_204"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }


        private void ComboTwitter_Selected(object sender, RoutedEventArgs e)
        {
            if (currentSong != null)
            {
                if(CheckForInternetConnection())
                {
                    string link = "https://twitter.com/intent/tweet?text=%23NowPlaying%20";

                    string tempArtist = HttpUtility.UrlEncode(currentSong.Artist.Name);
                    string tempSong = HttpUtility.UrlEncode(currentSong.Title);
                    tempSong = String.Concat(tempArtist, " - ", tempSong);

                    link = String.Concat(link, tempSong);

                    Window1 window1 = new Window1(link);
                    window1.Show();
                    this.IsEnabled = false;
                    this.Effect = new BlurEffect();

                    window1.Window1Closing += Window1_Window1Closing;
                }
                else
                {
                    MessageBox.Show("Internet connection is not available. \n Try again later.");
                }
                

            }

        }

        private void Window1_Window1Closing(Window1 a)
        {
            comboTwitter.IsSelected = false;
            a.twitterBrowse.Dispose();
            this.Effect = null;
            this.IsEnabled = true;
        }

        private void ListBoxSongs_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (var file in files)
                {
                    
                    if (file.Substring(file.LastIndexOf('.')+1)=="mp3")
                    {
                        string tempFolder = file.Substring(0, file.LastIndexOf(@"\"));

                        data.GetSingleSong(tempFolder, file);
                    }
                }

                listBoxSongs.Items.Refresh();
            }
        }

        private void TabPlayer_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                bool addedplus = false;
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (var file in files)
                {
                    string tempFolder = file.Substring(0, file.LastIndexOf(@"\"));

                    if (data.GetSingleSong(tempFolder, file) == 0) addedplus = true;

                }
                
                listBoxSongs.Items.Refresh();

                if(addedplus)
                {
                    currentSong = data.Songs.Last();
                    currentSource = data.Songs;

                    UpdatePlayer(currentSong);
                    player.URL = currentSong.Directory;
                    player.controls.play();
                }


            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Window2 window2 = new Window2();
            this.Effect = new BlurEffect();
            this.IsEnabled = false;
            if(window2.ShowDialog() == true)
            {
                if(window2.DialogResult == true)
                {
                    if(listBoxSongs.SelectedItem != null)
                    {

                        var selected = listBoxSongs.SelectedItems.Cast<Song>().ToArray();
                        foreach (Song temp in selected)
                        {
                            if (temp.Album != null)
                            {
                                temp.Album.Songs.Remove(temp);
                            }

                            if (temp.Album.Songs.Count == 0)
                            {
                                data.Albums.Remove(temp.Album);
                                listBoxAlbums.Items.Refresh();
                            }

                            data.Songs.Remove(temp);
                            listBoxSongs.Items.Refresh();
                        }


                    } 
                }
            }
            this.IsEnabled = true;
            this.Effect = null;
        }

        private void listBoxAlbums_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if(albumsStatus == false)
            {
                if(listBoxAlbums.SelectedItem != null)
                {
                    Album album = listBoxAlbums.SelectedItem as Album;
                    updateBack(album);
                }
            }
            
        }

        private void updateBack(Album album)
        {
            if(album!=null)
            {
                listBoxAlbums.Background = new ImageBrush(new BitmapImage(new Uri(album.Cover.Whole, UriKind.Relative)))
                {
                    Opacity = 0.25,
                    TileMode = TileMode.None,
                    Stretch = Stretch.UniformToFill,

                };
            }
           
        }


        private void goToAlbum_Click(object sender, RoutedEventArgs e)
        {
            TabAlbums.IsSelected = true;
            albumsStatus = true;

            listBoxAlbums.ItemsSource = null;

            Song song = (Song)listBoxSongs.SelectedItem;
            int index = data.Albums.IndexOf(song.Album);
           
            

            listBoxAlbums.ItemsSource = data.Albums[index].Songs;
            listBoxAlbums.DisplayMemberPath = "Title";

            updateBack(song.Album);


            buttonAlbBack.Visibility = Visibility.Visible;
        }

        private void addFiles_Click(object sender, RoutedEventArgs e)
        {
            addFilesDial();
            
        }

        private void addFilesDial()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if(dialog.SelectedPath!="")
                {
                    data.GetSongs(dialog.SelectedPath);

                    var selected = data.Albums.Cast<Album>().ToArray();
                    foreach (var album in selected)
                    {
                        if(album.Songs.Count<1)
                        {
                            data.Albums.Remove(album);
                        }
                    }

                    listBoxAlbums.Items.Refresh();
                    listBoxSongs.Items.Refresh();
                }
                
            }
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            writeVar();
            Application.Current.Shutdown();
        }

        private void writeVar()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            // получаем поток, куда будем записывать сериализованный объект
            using (FileStream fs = new FileStream("data.dat", FileMode.Create))
            {
                formatter.Serialize(fs, data);
            }

        }

        private void readVar()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream fs = new FileStream("data.dat", FileMode.Open))
            {
               data = (ProgramData)formatter.Deserialize(fs);

            }

            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            writeVar();
        }

        private void deleteAlbum_Click(object sender, RoutedEventArgs e)
        {
            Window2 window2 = new Window2();
            this.Effect = new BlurEffect();
            this.IsEnabled = false;
            if (albumsStatus==false)
            {
                if (window2.ShowDialog() == true)
                {
                    if (window2.DialogResult == true)
                    {
                        if (listBoxAlbums.SelectedItem != null)
                        {
                            
                            var selected = listBoxAlbums.SelectedItems.Cast<Album>().ToArray();
                            foreach (Album temp in selected)
                            {
                                foreach(Song var in temp.Songs)
                                {
                                    data.Songs.Remove(var);
                                }
                                temp.Songs.Clear();
                                data.Albums.Remove(temp);
                            }

                        }
                    }
                }
            }
            else
            {
                if (window2.ShowDialog() == true)
                {
                    if (window2.DialogResult == true)
                    {
                        if (listBoxSongs.SelectedItem != null)
                        {
                            var selected = listBoxAlbums.SelectedItems.Cast<Song>().ToArray();
                            foreach (Song temp in selected)
                            {
                                if (temp.Album != null)
                                {
                                    temp.Album.Songs.Remove(temp);
                                }

                                if (temp.Album.Songs.Count == 0)
                                {
                                    data.Albums.Remove(temp.Album);
                                    listBoxAlbums.Items.Refresh();
                                }

                                data.Songs.Remove(temp);
                            }


                        }
                    }
                }
            }


            listBoxAlbums.Items.Refresh();
            listBoxPlaylists.Items.Refresh();
            listBoxSongs.Items.Refresh();


            this.IsEnabled = true;
            this.Effect = null;
        }
    }

}
