using MahApps.Metro.Controls;
using wada.Views;

namespace wada
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            MainContentFrame.Content = new ProjectsView();
            BtnProjects.Click += (s, e) => MainContentFrame.Content = new ProjectsView();
            BtnClients.Click += (s, e) => MainContentFrame.Content = new ClientsView();
        }
    }
}