using MahApps.Metro.Controls;
using System.Windows;
using wada.Models;

namespace wada.Dialogs
{
    public partial class ClientDialog : MetroWindow
    {
        public string ClientName { get; private set; } = string.Empty;
        public string ClientContact { get; private set; } = string.Empty;
        public string ClientEmail { get; private set; } = string.Empty;

        public ClientDialog(ClientModel? existing = null)
        {
            InitializeComponent();
            if (existing != null)
            {
                Title = "Edit Client";
                TxtName.Text = existing.Name;
                TxtContact.Text = existing.MobileNumber;
                TxtEmail.Text = existing.Email;
            }
            else
            {
                Title = "Add Client";
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                MessageBox.Show("Client name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ClientName = TxtName.Text.Trim();
            ClientContact = TxtContact.Text.Trim();
            ClientEmail = TxtEmail.Text.Trim();
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}