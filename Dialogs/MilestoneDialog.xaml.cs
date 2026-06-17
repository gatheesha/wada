using MahApps.Metro.Controls;
using System.Windows;

namespace wada.Dialogs
{
    public partial class MilestoneDialog : MetroWindow
    {
        public string MilestoneDescription { get; private set; }
        public DateTime? Deadline { get; private set; }

        private readonly DateTime _projectDeadline;
        public double MilestonePrice { get; private set; }

        public MilestoneDialog(DateTime projectDeadline)
        {
            InitializeComponent();
            _projectDeadline = projectDeadline;

            // Set min/max selectable dates
            DpDeadline.DisplayDateStart = DateTime.Today;
            DpDeadline.DisplayDateEnd = projectDeadline.Date;
            DpDeadline.SelectedDate = DateTime.Today;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtDescription.Text))
            {
                MessageBox.Show("Milestone name is required.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtPrice.Text) || !double.TryParse(TxtPrice.Text, out double price) || price < 0)
            {
                MessageBox.Show("Please enter a valid non-negative price.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DpDeadline.SelectedDate == null)
            {
                MessageBox.Show("Please select a deadline.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DpDeadline.SelectedDate.Value.Date > _projectDeadline.Date)
            {
                MessageBox.Show(
                    $"Milestone deadline cannot exceed the project deadline ({_projectDeadline:yyyy-MM-dd}).",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MilestoneDescription = TxtDescription.Text.Trim();
            Deadline = DpDeadline.SelectedDate.Value;
            MilestonePrice = price;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}