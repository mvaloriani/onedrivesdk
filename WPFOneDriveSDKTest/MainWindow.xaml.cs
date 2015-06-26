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
using System.Windows.Navigation;
using System.Windows.Shapes;
using OneDrive;

namespace WPFOneDriveSDKTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private ODItem _sourceItem;
        private ODConnection _connection;
        private bool _selected;

        public ODConnection Connection
        {
            get { return _connection; }
            set
            {
                if (value == _connection)
                    return;
                _connection = value;
                //SourceItemChanged();
            }
        }

        public MainWindow()
        {

            Properties.Settings.Default.TestSettings = "questo e' un altro test";
            Properties.Settings.Default.Save();
            InitializeComponent();

            
        }

        private async Task SignIn()
        {
            Connection = await OAuthAuthenticator.SignInToMicrosoftAccount();
            //if (null != Connection)
            //{
            //    await LoadFolderFromId("root");
            //}
            //UpdateConnectedStateUx();
        }

        private void my(object sender, RoutedEventArgs e)
        {
            SignIn();
        }
    }
}
