using System.Windows;

namespace wada.Dialogs
{
    public partial class TaskDialog : Window
    {
        public string TaskName        { get; private set; }
        public string TaskDescription { get; private set; }
        public string Deadline        { get; private set; }

        public TaskDialog() => InitializeComponent();

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                MessageBox.Show("Task name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            TaskName        = TxtName.Text.Trim();
            TaskDescription = TxtDescription.Text.Trim();
            Deadline        = TxtDeadline.Text.Trim();
            DialogResult    = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
