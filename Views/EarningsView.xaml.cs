using System.Windows;
using System.Windows.Controls;
using wada.Dialogs;
using wada.Models;
using wada.ViewModels;

namespace wada.Views
{
    public partial class EarningsView : UserControl
    {
        public EarningsView()
        {
            InitializeComponent();
            ViewModel.RequestFinanceDialog += OnRequestFinanceDialog;
        }

        private void OnRequestFinanceDialog(FinanceModel? target, string defaultType)
        {
            var dialog = new FinanceDialog(target, defaultType) { Owner = Window.GetWindow(this) };
            if (dialog.ShowDialog() == true)
            {
                if (target == null)
                {
                    ViewModel.ConfirmAdd(dialog.Amount, dialog.FinanceType, dialog.Description, dialog.Date);
                }
                else
                {
                    ViewModel.ConfirmEdit(target, dialog.Amount, dialog.FinanceType, dialog.Description, dialog.Date);
                }
            }
        }
    }
}
