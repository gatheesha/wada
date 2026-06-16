using System.Windows;

namespace wada.Dialogs
{
    public partial class MilestoneDialog : Window
    {
        public string MilestoneDescription { get; private set; }
        public string Deadline             { get; private set; }

        public MilestoneDialog() => InitializeComponent();

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtDescription.Text))
            {
                MessageBox.Show("Milestone name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            MilestoneDescription = TxtDescription.Text.Trim();
            Deadline             = TxtDeadline.Text.Trim();
            DialogResult         = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
