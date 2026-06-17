using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using wada.Views;

namespace wada
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            BtnDashboard.Click += (s, e) => Navigate(new ProjectsView(),  BtnDashboard);
            BtnProjects.Click  += (s, e) => Navigate(new ProjectsView(),  BtnProjects);
            BtnClients.Click   += (s, e) => Navigate(new ClientsView(),   BtnClients);
            BtnEarnings.Click  += (s, e) => Navigate(new EarningsView(),  BtnEarnings);
            BtnInvoices.Click  += (s, e) => Navigate(new InvoicesView(),  BtnInvoices);

            Navigate(new ProjectsView(), BtnProjects);
        }

        private void Navigate(object view, Button active)
        {
            MainContentFrame.Content = view;

            foreach (var btn in new[] { BtnDashboard, BtnProjects, BtnClients, BtnEarnings, BtnInvoices, BtnReports })
                btn.Tag = null;

            active.Tag = "active";
        }
    }
}
