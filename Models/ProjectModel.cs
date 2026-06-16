using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace wada.Models
{
    public class ProjectModel : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _description = string.Empty;
        private string _status = string.Empty;
        private DateTime _startDate = DateTime.Today;
        private string _startTime = "09:00"; // Default start time string
        private int _durationDays = 7;       // Default project length

        public int Id { get; set; }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(); OnPropertyChanged(nameof(EndDateTime)); OnPropertyChanged(nameof(DeadlineText)); }
        }

        public string StartTime
        {
            get => _startTime;
            set { _startTime = value; OnPropertyChanged(); OnPropertyChanged(nameof(EndDateTime)); OnPropertyChanged(nameof(DeadlineText)); }
        }

        public int DurationDays
        {
            get => _durationDays;
            set { _durationDays = value; OnPropertyChanged(); OnPropertyChanged(nameof(EndDateTime)); OnPropertyChanged(nameof(DeadlineText)); }
        }

        // Calculates the absolute exact end date and time seamlessly
        public DateTime EndDateTime
        {
            get
            {
                if (!TimeSpan.TryParse(StartTime, out var timeSpan))
                {
                    timeSpan = new TimeSpan(9, 0, 0); // Fallback to 9 AM if parsing fails
                }
                return StartDate.Date.Add(timeSpan).AddDays(DurationDays);
            }
        }

        public string DeadlineText
        {
            get
            {
                TimeSpan remaining = EndDateTime - DateTime.Now;

                if (remaining.TotalSeconds <= 0) return "Overdue";
                if (remaining.TotalDays < 1) return $"{(int)remaining.TotalHours}h {remaining.Minutes}m left";
                return $"{(int)remaining.TotalDays}d {remaining.Hours}h left";
            }
        }

        public void RefreshCountdown() => OnPropertyChanged(nameof(DeadlineText));

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}