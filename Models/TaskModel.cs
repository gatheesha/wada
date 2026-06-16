using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace wada.Models
{
    internal class TaskModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public bool IsCompleted { get; set; }
        public int MilestoneId { get; set; }

        public string CountdownText
        {
            get
            {
                if (Deadline == DateTime.MinValue) return "";

                TimeSpan remaining = Deadline.Date.AddDays(1).AddSeconds(-1) - DateTime.Now;

                if (remaining.TotalSeconds <= 0) return "Overdue";
                if (remaining.TotalDays < 1) return $"{(int)remaining.TotalHours}h {remaining.Minutes}m left";
                return $"{(int)remaining.TotalDays}d {remaining.Hours}h left";
            }
        }

        public void RefreshCountdown() => OnPropertyChanged(nameof(CountdownText));

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}