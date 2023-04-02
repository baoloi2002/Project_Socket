using System;
using System.Collections.Generic;
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

// TODO
// 1. Add Game Menu: Play, Quit
// 2. Play Window: Server IP, Port, Nickname, Connect
// 3. If click Connect -> Game Window (which is already done)
// 4. Win Window: Win, Quit



namespace Project_Socket.Client
{
    /// <summary>
    /// Interaction logic for ClientWindow.xaml
    /// </summary>
    public partial class ClientGame : UserControl
    {
        public ClientGame()
        {
            InitializeComponent();
        }


        private void Choice_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Choose the question
            // Return dialog box
            MessageBox.Show("Hello World!");
            

        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Skip the question
            // Return dialog box
            MessageBox.Show("Hello World!");

        }
    }

}
