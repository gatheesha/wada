using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using wada.Data;
using wada.Models;

namespace wada.ViewModels
{
    internal class EarningsViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseContext _db;
        private FinanceModel? _selectedRecord;
        private string _activeFilter = "AllTime";   // "ThisWeek" | "ThisMonth" | "AllTime"

        // ── Collections ──────────────────────────────────────────────────────
        public ObservableCollection<FinanceModel> Records { get; } = new();

        // ── Summary stats ─────────────────────────────────────────────────────
        private double _totalEarnings;
        private double _totalExpenses;
        private double _pendingAmount;

        public double TotalEarnings
        {
            get => _totalEarnings;
            private set { _totalEarnings = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalEarningsDisplay)); OnPropertyChanged(nameof(NetBalance)); OnPropertyChanged(nameof(NetBalanceDisplay)); }
        }

        public double TotalExpenses
        {
            get => _totalExpenses;
            private set { _totalExpenses = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalExpensesDisplay)); OnPropertyChanged(nameof(NetBalance)); OnPropertyChanged(nameof(NetBalanceDisplay)); }
        }

        public double PendingAmount
        {
            get => _pendingAmount;
            private set { _pendingAmount = value; OnPropertyChanged(); OnPropertyChanged(nameof(PendingAmountDisplay)); }
        }

        public double NetBalance => TotalEarnings - TotalExpenses;

        public string TotalEarningsDisplay => $"${TotalEarnings:N2}";
        public string TotalExpensesDisplay => $"${TotalExpenses:N2}";
        public string PendingAmountDisplay => $"${PendingAmount:N2}";
        public string NetBalanceDisplay    => $"${NetBalance:N2}";

        // ── Filter label shown in UI ──────────────────────────────────────────
        public string ActiveFilter
        {
            get => _activeFilter;
            set { _activeFilter = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsFilterThisWeek)); OnPropertyChanged(nameof(IsFilterThisMonth)); OnPropertyChanged(nameof(IsFilterAllTime)); LoadRecords(); }
        }

        // Used to highlight the active filter button in the view
        public bool IsFilterThisWeek  => ActiveFilter == "ThisWeek";
        public bool IsFilterThisMonth => ActiveFilter == "ThisMonth";
        public bool IsFilterAllTime   => ActiveFilter == "AllTime";

        // ── Selection ─────────────────────────────────────────────────────────
        public FinanceModel? SelectedRecord
        {
            get => _selectedRecord;
            set { _selectedRecord = value; OnPropertyChanged(); }
        }

        // ── Commands ──────────────────────────────────────────────────────────
        public ICommand AddEarningCommand   { get; }
        public ICommand AddExpenseCommand   { get; }
        public ICommand AddPendingCommand   { get; }
        public ICommand EditRecordCommand   { get; }
        public ICommand DeleteRecordCommand { get; }

        public ICommand FilterThisWeekCommand  { get; }
        public ICommand FilterThisMonthCommand { get; }
        public ICommand FilterAllTimeCommand   { get; }

        // ── Dialog event ──────────────────────────────────────────────────────
        public event Action<FinanceModel?, string>? RequestFinanceDialog;

        // ─────────────────────────────────────────────────────────────────────
        public EarningsViewModel()
        {
            _db = new DatabaseContext();

            AddEarningCommand   = new RelayCommand(_ => RequestFinanceDialog?.Invoke(null, "Earning"));
            AddExpenseCommand   = new RelayCommand(_ => RequestFinanceDialog?.Invoke(null, "Expense"));
            AddPendingCommand   = new RelayCommand(_ => RequestFinanceDialog?.Invoke(null, "Pending"));
            EditRecordCommand   = new RelayCommand(_ => RequestFinanceDialog?.Invoke(SelectedRecord, SelectedRecord!.FinanceType), _ => SelectedRecord != null);
            DeleteRecordCommand = new RelayCommand(_ => OnDeleteRecord(), _ => SelectedRecord != null);

            FilterThisWeekCommand  = new RelayCommand(_ => ActiveFilter = "ThisWeek");
            FilterThisMonthCommand = new RelayCommand(_ => ActiveFilter = "ThisMonth");
            FilterAllTimeCommand   = new RelayCommand(_ => ActiveFilter = "AllTime");

            LoadRecords();
        }

        // ─────────────────────────────────────────────────────────────────────
        public void LoadRecords()
        {
            Records.Clear();

            var all = _db.GetAllFinance();

            // Apply time window
            var now   = DateTime.Now;
            var start = ActiveFilter switch
            {
                "ThisWeek"  => now.Date.AddDays(-(int)now.DayOfWeek),   // Sunday of this week
                "ThisMonth" => new DateTime(now.Year, now.Month, 1),
                _           => DateTime.MinValue
            };

            var windowed = ActiveFilter == "AllTime"
                ? all
                : all.Where(r => r.Date >= start).ToList();

            // Summaries reflect the current time window
            TotalEarnings = windowed.Where(r => r.FinanceType == "Earning").Sum(r => r.Amount);
            TotalExpenses = windowed.Where(r => r.FinanceType == "Expense").Sum(r => r.Amount);
            PendingAmount = windowed.Where(r => r.FinanceType == "Pending").Sum(r => r.Amount);

            foreach (var record in windowed)
                Records.Add(record);
        }

        private void OnDeleteRecord()
        {
            if (SelectedRecord == null) return;

            var result = MessageBox.Show(
                $"Delete this {SelectedRecord.FinanceType.ToLower()} of ${SelectedRecord.Amount:N2}?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _db.DeleteFinance(SelectedRecord.Id);
                LoadRecords();
                SelectedRecord = null;
            }
        }

        public void ConfirmAdd(double amount, string financeType, string description, DateTime date)
        {
            _db.AddFinance(amount, 0, date.ToString("yyyy-MM-dd"), financeType, description);
            LoadRecords();
        }

        public void ConfirmEdit(FinanceModel record, double amount, string financeType, string description, DateTime date)
        {
            record.Amount      = amount;
            record.FinanceType = financeType;
            record.Description = description;
            record.Date        = date;
            _db.UpdateFinance(record);
            LoadRecords();
        }

        // ─────────────────────────────────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
