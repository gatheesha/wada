using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using wada.Data;
using wada.Models;

namespace wada.ViewModels
{
    internal class ClientsViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseContext _db;
        private string _searchText = string.Empty;
        private ClientModel? _selectedClient;

        public ObservableCollection<ClientModel> Clients { get; set; } = new();

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                LoadClients(); // Triggers real-time filtering whenever text updates
            }
        }

        public ClientModel? SelectedClient
        {
            get => _selectedClient;
            set { _selectedClient = value; OnPropertyChanged(); }
        }

        // View action triggers handled in UI code-behind layer
        public event Action<ClientModel?>? RequestClientDialog;

        public ICommand AddClientCommand { get; }
        public ICommand EditClientCommand { get; }
        public ICommand DeleteClientCommand { get; }

        public ClientsViewModel()
        {
            _db = new DatabaseContext();

            AddClientCommand = new RelayCommand(_ => RequestClientDialog?.Invoke(null));
            EditClientCommand = new RelayCommand(_ => RequestClientDialog?.Invoke(SelectedClient), _ => SelectedClient != null);
            DeleteClientCommand = new RelayCommand(_ => OnDeleteClient(), _ => SelectedClient != null);

            LoadClients();
        }

        public void LoadClients()
        {
            Clients.Clear();
            var results = string.IsNullOrWhiteSpace(SearchText)
                ? _db.GetAllClients()
                : _db.FilterClient(SearchText); // Calls native SQLite wildcard parameter filters

            foreach (var client in results)
            {
                Clients.Add(client);
            }
        }

        private void OnDeleteClient()
        {
            if (SelectedClient == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete {SelectedClient.Name}?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _db.DeleteClient(SelectedClient.Id);
                LoadClients();
                SelectedClient = null;
            }
        }

        public void ConfirmAddClient(string name, string contact, string email)
        {
            _db.AddClient(name, contact, email);
            LoadClients();
        }

        public void ConfirmEditClient(ClientModel client, string name, string contact, string email)
        {
            client.Name = name;
            client.MobileNumber = contact;
            client.Email = email;
            _db.UpdateClient(client);
            LoadClients();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}