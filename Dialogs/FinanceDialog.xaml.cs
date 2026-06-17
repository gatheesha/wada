using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using MahApps.Metro.Controls;
using wada.Models;

namespace wada.Dialogs
{
    public partial class FinanceDialog : MetroWindow, INotifyPropertyChanged
    {
        // ── Bindable properties (DataContext = this) ──────────────────────────
        private string _windowTitle = "Add Record";
        private string _financeType = "Earning";
        private double _amount;
        private string _description = string.Empty;
        private DateTime _date = DateTime.Today;

        public string WindowTitle
        {
            get => _windowTitle;
            set { _windowTitle = value; OnPropertyChanged(); }
        }

        public string FinanceType
        {
            get => _financeType;
            set { _financeType = value; OnPropertyChanged(); }
        }

        public double Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); }
        }

        // ─────────────────────────────────────────────────────────────────────
        /// <param name="target">Null → Add mode; non-null → Edit mode.</param>
        /// <param name="defaultType">Pre-select this type when adding.</param>
        public FinanceDialog(FinanceModel? target, string defaultType = "Earning")
        {
            InitializeComponent();
            DataContext = this;

            if (target != null)
            {
                WindowTitle  = "Edit Record";
                FinanceType  = target.FinanceType;
                Amount       = target.Amount;
                Description  = target.Description;
                Date         = target.Date;
            }
            else
            {
                FinanceType = defaultType;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (Amount <= 0)
            {
                MessageBox.Show("Please enter an amount greater than zero.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
            => DialogResult = false;

        // ─────────────────────────────────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
