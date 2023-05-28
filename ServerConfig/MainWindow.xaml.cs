
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace ServerConfig
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var serv = new Server();
            Task initTask = serv.Init();

            if (initTask == null || initTask.Status == TaskStatus.Faulted)
            {
                Debug.WriteLine("error with Init Server");
            }
        }
    }
}
