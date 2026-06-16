using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using wada.Data;
using wada.Models;

namespace wada.ViewModels
{
    // ─────────────────────────────────────────────────────────────
    //  Flat list item — either a Milestone header or a Task row
    // ─────────────────────────────────────────────────────────────
    internal class ProjectDetailItem : INotifyPropertyChanged
    {
        public bool IsMilestoneHeader { get; set; }

        // Milestone fields (used when IsMilestoneHeader = true)
        public MilestoneModel Milestone { get; set; }

        // Task fields (used when IsMilestoneHeader = false)
        public TaskModel Task { get; set; }

        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set { _isCompleted = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // ─────────────────────────────────────────────────────────────
    //  ProjectsViewModel
    // ─────────────────────────────────────────────────────────────
    internal class ProjectsViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseContext _db = new DatabaseContext();

        // ── Project list (left panel) ──────────────────────────
        public ObservableCollection<ProjectModel> Projects { get; } = new();

        private ProjectModel _selectedProject;
        public ProjectModel SelectedProject
        {
            get => _selectedProject;
            set
            {
                _selectedProject = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDetailVisible));
                LoadDetailItems();
                LoadLinkedClients();
            }
        }

        public bool IsDetailVisible => _selectedProject != null;

        // ── Right panel flat list ──────────────────────────────
        public ObservableCollection<ProjectDetailItem> DetailItems { get; } = new();

        // ── Linked clients shown in detail panel ───────────────
        public ObservableCollection<ClientModel> LinkedClients { get; } = new();

        // ── All clients (for dropdowns in dialogs) ─────────────
        public ObservableCollection<ClientModel> AllClients { get; } = new();

        // ── Search ─────────────────────────────────────────────
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                LoadProjects(); // Forces left pane to refresh instantly when typing
            }
        }

        // ── Commands ───────────────────────────────────────────
        public ICommand AddProjectCommand { get; }
        public ICommand EditProjectCommand { get; }
        public ICommand DeleteProjectCommand { get; }
        public ICommand AddMilestoneCommand { get; }
        public ICommand AddTaskCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand ToggleTaskCommand { get; }

        // ── Dialog-request events (Views subscribe to these) ───
        public event Action<ProjectModel, List<ClientModel>, List<ClientModel>> RequestAddProjectDialog;
        public event Action<ProjectModel, List<ClientModel>, List<ClientModel>> RequestEditProjectDialog;
        public event Action<int, DateTime> RequestAddMilestoneDialog;   // projectId, projectDeadline
        public event Action<int, DateTime> RequestAddTaskDialog;         // milestoneId, milestoneDeadline

        public ProjectsViewModel()
        {
            AddProjectCommand = new RelayCommand(_ => OnAddProject());
            EditProjectCommand = new RelayCommand(_ => OnEditProject(), _ => SelectedProject != null);
            DeleteProjectCommand = new RelayCommand(_ => OnDeleteProject(), _ => SelectedProject != null);
            AddMilestoneCommand = new RelayCommand(_ => OnAddMilestone(), _ => SelectedProject != null);
            AddTaskCommand = new RelayCommand(item => OnAddTask(item), _ => SelectedProject != null);
            DeleteItemCommand = new RelayCommand(item => OnDeleteItem(item));
            ToggleTaskCommand = new RelayCommand(item => OnToggleTask(item));

            LoadProjects();
            LoadAllClients();

            // ── REAL TIME ENGINE INITIALIZATION ──
            DispatcherTimer liveTimer = new DispatcherTimer();
            liveTimer.Interval = TimeSpan.FromSeconds(1);
            liveTimer.Tick += (s, e) =>
            {
                // Refresh left panel project countdowns
                foreach (var project in Projects)
                {
                    project.RefreshCountdown();
                }

                // Refresh right panel flat list task countdowns
                foreach (var detailItem in DetailItems)
                {
                    if (!detailItem.IsMilestoneHeader && detailItem.Task != null)
                    {
                        detailItem.Task.RefreshCountdown();
                    }
                }
            };
            liveTimer.Start();
        }

        // ── Data loading ───────────────────────────────────────

        public void LoadProjects()
        {
            Projects.Clear();

            // Check if user is actively performing a keyword query search filter
            List<ProjectModel> list;
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                list = _db.GetAllProjects();
            }
            else
            {
                // Pulls matches seamlessly using your built-in Database query filter context engine
                list = _db.FilterProjects(SearchText);
            }

            foreach (var p in list)
            {
                Projects.Add(p);
            }
        }

        private void LoadAllClients()
        {
            AllClients.Clear();
            foreach (var c in _db.GetAllClients()) AllClients.Add(c);
        }

        private void LoadDetailItems()
        {
            DetailItems.Clear();
            if (_selectedProject == null) return;

            var milestones = _db.GetMilestonesByProject(_selectedProject.Id);
            foreach (var m in milestones)
            {
                // Milestone header row
                DetailItems.Add(new ProjectDetailItem
                {
                    IsMilestoneHeader = true,
                    Milestone = m
                });

                // Tasks under this milestone
                var tasks = _db.GetTasksByMilestone(m.Id);
                foreach (var t in tasks)
                {
                    DetailItems.Add(new ProjectDetailItem
                    {
                        IsMilestoneHeader = false,
                        Task = t,
                        IsCompleted = t.IsCompleted
                    });
                }
            }
        }

        private void LoadLinkedClients()
        {
            LinkedClients.Clear();
            if (_selectedProject == null) return;
            foreach (var c in _db.GetClientsByProject(_selectedProject.Id))
                LinkedClients.Add(c);
        }

        private void Search()
        {
            var prev = _selectedProject;
            LoadProjects();
            // Try to keep selection if it still exists after filter
            SelectedProject = Projects.FirstOrDefault(p => p.Id == prev?.Id);
        }

        // ── Command handlers ───────────────────────────────────

        private void OnAddProject()
        {
            LoadAllClients();
            RequestAddProjectDialog?.Invoke(null, AllClients.ToList(), new List<ClientModel>());
        }

        private void OnEditProject()
        {
            if (_selectedProject == null) return;
            LoadAllClients();
            var linked = _db.GetClientsByProject(_selectedProject.Id);
            RequestEditProjectDialog?.Invoke(_selectedProject, AllClients.ToList(), linked);
        }

        private void OnDeleteProject()
        {
            if (_selectedProject == null) return;
            _db.DeleteProject(_selectedProject.Id);
            SelectedProject = null;
            LoadProjects();
        }

        private void OnAddMilestone()
        {
            if (_selectedProject == null) return;
            RequestAddMilestoneDialog?.Invoke(_selectedProject.Id, _selectedProject.EndDateTime);
        }

        private void OnAddTask(object param)
        {
            if (_selectedProject == null) return;
            if (param is int milestoneId)
            {
                // Find the milestone deadline from the current detail items
                var milestoneItem = DetailItems.FirstOrDefault(
                    d => d.IsMilestoneHeader && d.Milestone.Id == milestoneId);
                var milestoneDeadline = milestoneItem?.Milestone.Deadline != DateTime.MinValue
                    ? milestoneItem.Milestone.Deadline
                    : _selectedProject.EndDateTime; // fallback to project deadline
                RequestAddTaskDialog?.Invoke(milestoneId, milestoneDeadline);
            }
        }

        private void OnDeleteItem(object param)
        {
            if (param is ProjectDetailItem item)
            {
                if (item.IsMilestoneHeader)
                    _db.DeleteMilestone(item.Milestone.Id);
                else
                    _db.DeleteTask(item.Task.Id);

                LoadDetailItems();
            }
        }

        private void OnToggleTask(object param)
        {
            if (param is ProjectDetailItem item && !item.IsMilestoneHeader)
            {
                item.IsCompleted = !item.IsCompleted;
                item.Task.IsCompleted = item.IsCompleted;
                _db.SetTaskCompleted(item.Task.Id, item.IsCompleted);
            }
        }

        // ── Called by the View after dialog confirms ───────────

        /// <summary>Called by dialog after user confirms adding a project.</summary>
        public void ConfirmAddProject(string name, string description, string startDate, string startTime, int durationDays, string status, List<int> clientIds)
        {
            _db.AddProject(name, description, startDate, startTime, durationDays, status, clientIds);
            Console.WriteLine("New project added");
            LoadProjects(); // Instantly reloads the UI panel list view
        }

        /// <summary>Called by dialog after user confirms editing a project.</summary>
        public void ConfirmEditProject(ProjectModel updated, List<int> newClientIds, List<int> removedClientIds)
        {
            _db.UpdateProject(updated);

            foreach (var cid in removedClientIds)
                _db.UnlinkClientFromProject(cid, updated.Id);
            foreach (var cid in newClientIds)
                _db.LinkClientToProject(cid, updated.Id);

            LoadProjects();
            SelectedProject = Projects.FirstOrDefault(p => p.Id == updated.Id);
        }

        /// <summary>Called by dialog after user confirms adding a milestone.</summary>
        public void ConfirmAddMilestone(int projectId, string description, DateTime deadline)
        {
            _db.AddMilestone(projectId, description, 0, deadline.ToString("yyyy-MM-dd"));
            LoadDetailItems();
        }

        /// <summary>Called by dialog after user confirms adding a task.</summary>
        public void ConfirmAddTask(int milestoneId, string name, string description, DateTime deadline)
        {
            _db.AddTask(milestoneId, name, description, deadline.ToString("yyyy-MM-dd"));
            LoadDetailItems();
        }

        // ── INotifyPropertyChanged ─────────────────────────────

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // ─────────────────────────────────────────────────────────────
    //  Generic RelayCommand
    // ─────────────────────────────────────────────────────────────
    internal class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}