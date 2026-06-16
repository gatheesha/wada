using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using wada.Models;

namespace wada.Dialogs
{
    public partial class ProjectDialog : MetroWindow
    {
        public string ProjectName { get; private set; }
        public string ProjectDescription { get; private set; }
        public string StartDate { get; private set; } = string.Empty;
        public string StartTime { get; private set; } = string.Empty;
        public int DurationDays { get; private set; } = 7;
        public string Status { get; private set; }
        public List<int> SelectedClientIds { get; private set; } = new();

        public ProjectDialog(ProjectModel? existing, List<ClientModel> allClients, List<ClientModel> linkedClients)
        {
            InitializeComponent();
            LstClients.ItemsSource = allClients;

            if (existing != null)
            {
                Title = "Edit Project";
                TxtName.Text = existing.Name;
                TxtDescription.Text = existing.Description;
                DpStartDate.SelectedDate = existing.StartDate;
                TxtStartTime.Text = existing.StartTime;
                NudDuration.Value = existing.DurationDays;

                foreach (ComboBoxItem item in CmbStatus.Items)
                    if (item.Content.ToString() == existing.Status)
                    { CmbStatus.SelectedItem = item; break; }

                // Pre-select linked clients
                foreach (var client in allClients)
                    if (linkedClients.Any(lc => lc.Id == client.Id))
                        LstClients.SelectedItems.Add(client);
            }
            else
            {
                Title = "Add Project";
                DpStartDate.SelectedDate = DateTime.Today;
                CmbStatus.SelectedIndex = 0;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                MessageBox.Show("Project name is required.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ProjectName = TxtName.Text.Trim();
            ProjectDescription = TxtDescription.Text.Trim();
            StartDate = DpStartDate.SelectedDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");
            StartTime = string.IsNullOrWhiteSpace(TxtStartTime.Text) ? "09:00" : TxtStartTime.Text.Trim();
            DurationDays = (int)(NudDuration.Value ?? 7);
            Status = (CmbStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Active";
            SelectedClientIds = LstClients.SelectedItems.Cast<ClientModel>().Select(c => c.Id).ToList();

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}