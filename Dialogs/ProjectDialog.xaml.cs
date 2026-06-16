using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using wada.Models;

namespace wada.Dialogs
{
    public partial class ProjectDialog : Window
    {
        // Output properties read by the caller
        public string ProjectName { get; private set; }
        public string ProjectDescription { get; private set; }
        public string StartDate { get; private set; }
        public string EndDate { get; private set; }
        public string Status { get; private set; }
        public List<int> SelectedClientIds { get; private set; } = new();

        private readonly List<ClientModel> _allClients;

        /// <param name="existing">Pass null for Add, or the project to pre-fill for Edit.</param>
        public ProjectDialog(ProjectModel existing, List<ClientModel> allClients, List<ClientModel> linkedClients)
        {
            InitializeComponent();
            _allClients = allClients;

            // Populate client list
            LstClients.ItemsSource = allClients;

            if (existing != null)
            {
                Title = "Edit Project";
                TxtName.Text = existing.Name;
                TxtDescription.Text = existing.Description;
                TxtStartDate.Text = existing.StartDate.ToString("yyyy-MM-dd");
                TxtEndDate.Text = existing.EndDate.ToString("yyyy-MM-dd");

                // Pre-select status
                foreach (ComboBoxItem item in CmbStatus.Items)
                    if (item.Content.ToString() == existing.Status)
                    { CmbStatus.SelectedItem = item; break; }

                // Pre-select linked clients
                var linkedIds = linkedClients.Select(c => c.Id).ToHashSet();
                foreach (var client in allClients)
                {
                    if (linkedIds.Contains(client.Id))
                        LstClients.SelectedItems.Add(client);
                }
            }
            else
            {
                Title = "Add Project";
                CmbStatus.SelectedIndex = 0;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                MessageBox.Show("Project name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ProjectName = TxtName.Text.Trim();
            ProjectDescription = TxtDescription.Text.Trim();
            StartDate = TxtStartDate.Text.Trim();
            EndDate = TxtEndDate.Text.Trim();
            Status = (CmbStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";
            SelectedClientIds = LstClients.SelectedItems.Cast<ClientModel>().Select(c => c.Id).ToList();

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}