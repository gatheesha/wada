using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using wada.Data;
using wada.Models;

namespace wada.ViewModels
{
    internal class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseContext _db = new DatabaseContext();
        private string _activeFilter = "ThisMonth";

        public ObservableCollection<FinanceModel> RecentRecords { get; } = new();
        public ObservableCollection<TaskModel> UpcomingTasks { get; } = new();
        public ObservableCollection<ProjectModel> ActiveProjects { get; } = new();

        // LiveCharts
        public SeriesCollection EarningsSeries { get; set; } = new();
        public List<string> XLabels { get; set; } = new();

        public ICommand SetThisMonthCommand => new RelayCommand(_ => ActiveFilter = "Month");
        public ICommand SetThisYearCommand => new RelayCommand(_ => ActiveFilter = "Year");
        public ICommand SetAllTimeCommand => new RelayCommand(_ => ActiveFilter = "Time");

        public string ActiveFilter
        {
            get => _activeFilter;
            set { _activeFilter = value; OnPropertyChanged(); LoadData(); }
        }

        public Func<double, string> CurrencyFormatter { get; } = value => value.ToString("C0");
        public bool IsThisMonth => ActiveFilter == "ThisMonth";
        public bool IsThisYear => ActiveFilter == "ThisYear";
        public bool IsAllTime => ActiveFilter == "AllTime";

        public DashboardViewModel()
        {
            LoadData();
        }

        public void LoadData()
        {
            LoadRecentFinance();
            LoadUpcomingTasks();
            LoadEarningsChart();
            LoadActiveProjects();
        }

        private void LoadRecentFinance()
        {
            RecentRecords.Clear();
            var all = _db.GetAllFinance()
                         .OrderByDescending(f => f.Date)
                         .Take(5);
            foreach (var r in all) RecentRecords.Add(r);
        }

        private void LoadUpcomingTasks()
        {
            UpcomingTasks.Clear();
            var now = DateTime.Now.Date;  // Use .Date to ignore time

            var allProjects = _db.GetAllProjects();

            foreach (var proj in allProjects)
            {
                var milestones = _db.GetMilestonesByProject(proj.Id);
                foreach (var m in milestones)
                {
                    var tasks = _db.GetTasksByMilestone(m.Id)
                                   .Where(t => !t.IsCompleted
                                               && t.Deadline != DateTime.MinValue
                                               && t.Deadline.Date >= now)
                                   .OrderBy(t => t.Deadline)
                                   .ToList();

                    foreach (var t in tasks.Take(10))
                        UpcomingTasks.Add(t);
                }
            }

            // Fallback message if still empty
            if (UpcomingTasks.Count == 0)
            {
                // You can optionally add a dummy item for UI testing, or just leave it empty
                System.Diagnostics.Debug.WriteLine("No upcoming tasks found.");
            }
        }

        private void LoadActiveProjects()
        {
            ActiveProjects.Clear();
            var projects = _db.GetAllProjects()
                              .Where(p => p.Status == "Active" || string.IsNullOrEmpty(p.Status))
                              .OrderBy(p => p.EndDateTime)
                              .ToList();

            foreach (var p in projects.Take(8))
                ActiveProjects.Add(p);
        }
        private void LoadEarningsChart()
        {
            EarningsSeries.Clear();
            XLabels.Clear();

            var allFinance = _db.GetAllFinance();

            var now = DateTime.Now;
            var filtered = ActiveFilter switch
            {
                "ThisYear" => allFinance.Where(f => f.Date.Year == now.Year).ToList(),
                "ThisMonth" => allFinance.Where(f => f.Date.Year == now.Year && f.Date.Month == now.Month).ToList(),
                _ => allFinance
            };

            // Group by Month for chart
            var monthly = filtered
                .GroupBy(f => new { f.Date.Year, f.Date.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Earnings = g.Where(f => f.FinanceType == "Earning").Sum(f => f.Amount),
                    Expenses = g.Where(f => f.FinanceType == "Expense").Sum(f => f.Amount)
                })
                .ToList();

            var earningsValues = new ChartValues<double>(monthly.Select(m => m.Earnings));
            var expenseValues = new ChartValues<double>(monthly.Select(m => m.Expenses));

            EarningsSeries.Add(new LineSeries
            {
                Title = "Earnings",
                Values = earningsValues,
                PointGeometry = DefaultGeometries.Circle,
                Stroke = System.Windows.Media.Brushes.LimeGreen,
                Fill = System.Windows.Media.Brushes.Transparent
            });

            EarningsSeries.Add(new LineSeries
            {
                Title = "Expenses",
                Values = expenseValues,
                PointGeometry = DefaultGeometries.Circle,
                Stroke = System.Windows.Media.Brushes.IndianRed,
                Fill = System.Windows.Media.Brushes.Transparent
            });

            XLabels = monthly.Select(m => m.Label).ToList();

            OnPropertyChanged(nameof(EarningsSeries));
            OnPropertyChanged(nameof(XLabels));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}