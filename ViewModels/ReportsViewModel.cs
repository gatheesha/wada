using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using wada.Data;

namespace wada.ViewModels
{
    internal class ReportsViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseContext _db = new DatabaseContext();
        private string _activeFilter = "ThisYear";

        public SeriesCollection Series { get; set; } = new();
        public List<string> Labels { get; set; } = new();
        public Func<double, string> CurrencyFormatter { get; } = value => value.ToString("C0");

        public string ActiveFilter
        {
            get => _activeFilter;
            set { _activeFilter = value; OnPropertyChanged(); LoadChart(); }
        }

        public ICommand SetThisMonthCommand => new RelayCommand(_ => ActiveFilter = "ThisMonth");
        public ICommand SetThisYearCommand => new RelayCommand(_ => ActiveFilter = "ThisYear");
        public ICommand SetAllTimeCommand => new RelayCommand(_ => ActiveFilter = "AllTime");

        public ReportsViewModel()
        {
            LoadChart();
        }

        private void LoadChart()
        {
            Series.Clear();
            Labels.Clear();

            var all = _db.GetAllFinance();

            var filtered = ActiveFilter switch
            {
                "ThisMonth" => all.Where(f => f.Date.Year == DateTime.Now.Year &&
                                             f.Date.Month == DateTime.Now.Month),
                "ThisYear" => all.Where(f => f.Date.Year == DateTime.Now.Year),
                _ => all
            };

            var monthly = filtered
                .GroupBy(f => new { f.Date.Year, f.Date.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Label = g.Key.Month == DateTime.Now.Month && g.Key.Year == DateTime.Now.Year
                            ? "Current"
                            : $"{g.Key.Year}-{g.Key.Month:D2}",
                    Earnings = g.Where(f => f.FinanceType == "Earning").Sum(f => f.Amount),
                    Expenses = g.Where(f => f.FinanceType == "Expense").Sum(f => f.Amount)
                })
                .ToList();

            Series.Add(new LineSeries
            {
                Title = "Earnings",
                Values = new ChartValues<double>(monthly.Select(m => m.Earnings)),
                Stroke = System.Windows.Media.Brushes.LimeGreen,
                PointGeometry = DefaultGeometries.Circle
            });

            Series.Add(new LineSeries
            {
                Title = "Expenses",
                Values = new ChartValues<double>(monthly.Select(m => m.Expenses)),
                Stroke = System.Windows.Media.Brushes.IndianRed,
                PointGeometry = DefaultGeometries.Circle
            });

            Labels = monthly.Select(m => m.Label).ToList();

            OnPropertyChanged(nameof(Series));
            OnPropertyChanged(nameof(Labels));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}