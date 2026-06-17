using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using wada.Data;
using wada.Models;
using wada.Services;
using wada.ViewModels;

namespace wada.Views
{
    public partial class InvoicesView : UserControl
    {
        private InvoicesViewModel _vm => (InvoicesViewModel)DataContext;

        public InvoicesView()
        {
            InitializeComponent();
            Loaded += (_, __) => _vm.RequestGenerateInvoice += OnGenerateInvoice;
        }

        private void OnGenerateInvoice(ProjectModel project)
        {
            var db = new DatabaseContext();

            var clients    = db.GetClientsByProject(project.Id);
            var milestones = db.GetMilestonesByProject(project.Id);

            // Save As dialog
            var dlg = new SaveFileDialog
            {
                Title            = "Save Invoice PDF",
                Filter           = "PDF files (*.pdf)|*.pdf",
                FileName         = $"Invoice_{project.Name.Replace(" ", "_")}_{System.DateTime.Now:yyyyMMdd}.pdf",
                DefaultExt       = ".pdf",
                InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
            };

            if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;

            try
            {
                InvoicePdfService.Generate(_vm.Profile, project, clients, milestones, dlg.FileName);
                MessageBox.Show($"Invoice saved to:\n{dlg.FileName}", "Invoice Generated",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Failed to generate invoice:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
