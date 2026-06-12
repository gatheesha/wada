using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using wada.Data;
using wada.Models;

namespace wada.ViewModels
{
    internal partial class MainViewModel : ObservableObject
    {
        private readonly DatabaseContext _dbContext;

        [ObservableProperty]
        private string _newName;

        [ObservableProperty]
        private string _newEmail;

        public ObservableCollection<UserModel> Users { get; set; }

        public MainViewModel()
        {
            _dbContext = new DatabaseContext();
            Users = new ObservableCollection<UserModel>(_dbContext.GetUsers());

            AddUserCommand = new RelayCommand(AddUser);
        }

        public ICommand AddUserCommand { get; }

        private void AddUser()
        {
            if (string.IsNullOrWhiteSpace(NewName) || string.IsNullOrWhiteSpace(NewEmail)) return;

            // Save to Database
            _dbContext.AddUser(NewName, NewEmail);

            // Refresh UI list
            Users.Clear();
            foreach (var user in _dbContext.GetUsers())
            {
                Users.Add(user);
            }

            // Clear Input Fields
            NewName = string.Empty;
            NewEmail = string.Empty;
        }
    }


}
