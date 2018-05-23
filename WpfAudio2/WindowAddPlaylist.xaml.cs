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

namespace WpfAudio2
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class WindowAddPlaylist : Window
    {
        public delegate void typFunkcji(WindowAddPlaylist a);
        public event typFunkcji WindowClose;

        public WindowAddPlaylist()
        {
            InitializeComponent();

            
        }
        
        public string title()
        {
            return textBox.Text;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (WindowClose != null)
            {
                WindowClose(this);
                
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Owner.Effect = null;
            Owner.IsEnabled = true;
            Close();
        }
    }
}
