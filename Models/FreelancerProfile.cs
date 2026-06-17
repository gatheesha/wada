using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace wada.Models
{
    /// <summary>Persisted to a small JSON file next to the database.</summary>
    public class FreelancerProfile : INotifyPropertyChanged
    {
        private string _name        = string.Empty;
        private string _businessName = string.Empty;
        private string _email       = string.Empty;
        private string _phone       = string.Empty;
        private string _address     = string.Empty;

        public string Name         { get => _name;         set { _name = value;         OnPropertyChanged(); } }
        public string BusinessName { get => _businessName; set { _businessName = value; OnPropertyChanged(); } }
        public string Email        { get => _email;        set { _email = value;        OnPropertyChanged(); } }
        public string Phone        { get => _phone;        set { _phone = value;        OnPropertyChanged(); } }
        public string Address      { get => _address;      set { _address = value;      OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string n = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
