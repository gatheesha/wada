using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace wada.Models
{
    public class FinanceModel : INotifyPropertyChanged
    {
        private double _amount;
        private string _financeType = string.Empty;
        private string _description = string.Empty;
        private DateTime _date = DateTime.Today;
        private int _projectId;

        public int Id { get; set; }

        public double Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(); OnPropertyChanged(nameof(AmountDisplay)); }
        }

        public string FinanceType
        {
            get => _financeType;
            set { _financeType = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); OnPropertyChanged(nameof(DateDisplay)); }
        }

        public int ProjectId
        {
            get => _projectId;
            set { _projectId = value; OnPropertyChanged(); }
        }

        // Display helpers
        public string DateDisplay => Date.ToString("MMM d, yyyy");

        public string AmountDisplay => FinanceType switch
        {
            "Expense" => $"-${Amount:N2}",
            _ => $"+${Amount:N2}"
        };

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
