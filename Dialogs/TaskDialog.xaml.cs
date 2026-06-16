using MahApps.Metro.Controls;
using System;
using System.Windows;

namespace wada.Dialogs
{
    public partial class TaskDialog : MetroWindow
    {
        public string TaskName { get; private set; }
        public string TaskDescription { get; private set; }
        public DateTime? Deadline { get; private set; }

        private readonly DateTime _milestoneDeadline;

        public TaskDialog(DateTime milestoneDeadline)
        {
            InitializeComponent();
            _milestoneDeadline = milestoneDeadline;

            DpDeadline.DisplayDateStart = DateTime.Today;
            DpDeadline.DisplayDateEnd = milestoneDeadline.Date;
            DpDeadline.SelectedDate = DateTime.Today;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                MessageBox.Show("Task name is required.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DpDeadline.SelectedDate == null)
            {
                MessageBox.Show("Please select a deadline.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DpDeadline.SelectedDate.Value.Date > _milestoneDeadline.Date)
            {
                MessageBox.Show(
                    $"Task deadline cannot exceed the milestone deadline ({_milestoneDeadline:yyyy-MM-dd}).",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TaskName = TxtName.Text.Trim();
            TaskDescription = TxtDescription.Text.Trim();
            Deadline = DpDeadline.SelectedDate.Value;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}