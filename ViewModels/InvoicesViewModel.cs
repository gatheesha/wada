using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using wada.Data;
using wada.Models;

namespace wada.ViewModels
{
    internal class InvoicesViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseContext _db;
        private ProjectModel? _selectedProject;

        public ObservableCollection<ProjectModel> Projects { get; } = new();

        public ProjectModel? SelectedProject
        {
            get => _selectedProject;
            set { _selectedProject = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanGenerate)); }
        }

        public bool CanGenerate => SelectedProject != null;

        // ── Profile ───────────────────────────────────────────────────────────
        public FreelancerProfile Profile { get; }

        // ── Commands ──────────────────────────────────────────────────────────
        public ICommand GenerateInvoiceCommand { get; }
        public ICommand SaveProfileCommand     { get; }

        // ── Event fired to code-behind (Save As dialog lives in the View) ─────
        public event Action<ProjectModel>? RequestGenerateInvoice;

        public InvoicesViewModel()
        {
            _db     = new DatabaseContext();
            Profile = ProfileStore.Load();

            GenerateInvoiceCommand = new RelayCommand(
                _ => RequestGenerateInvoice?.Invoke(SelectedProject!),
                _ => CanGenerate);

            SaveProfileCommand = new RelayCommand(_ =>
            {
                ProfileStore.Save(Profile);
            });

            LoadProjects();
        }

        private void LoadProjects()
        {
            Projects.Clear();
            foreach (var p in _db.GetAllProjects())
                Projects.Add(p);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string n = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
