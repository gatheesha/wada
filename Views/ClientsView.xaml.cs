using System.Windows;
using System.Windows.Controls;
using wada.Dialogs;
using wada.Models;
using wada.ViewModels;

namespace wada.Views
{
    public partial class ClientsView : UserControl
    {
        public ClientsView()
        {
            InitializeComponent();
            ViewModel.RequestClientDialog += OnRequestClientDialog;
        }

        private void OnRequestClientDialog(ClientModel? target)
        {
            var dialog = new ClientDialog(target) { Owner = Window.GetWindow(this) };
            if (dialog.ShowDialog() == true)
            {
                if (target == null)
                {
                    ViewModel.ConfirmAddClient(dialog.ClientName, dialog.ClientContact, dialog.ClientEmail);
                }
                else
                {
                    ViewModel.ConfirmEditClient(target, dialog.ClientName, dialog.ClientContact, dialog.ClientEmail);
                }
            }
        }
    }
}