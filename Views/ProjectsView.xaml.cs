using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using wada.Dialogs;
using wada.Models;
using wada.ViewModels;

namespace wada.Views
{
    public partial class ProjectsView : UserControl
    {
        private ProjectsViewModel _vm => (ProjectsViewModel)DataContext;

        public ProjectsView()
        {
            InitializeComponent();
            Loaded += (_, __) => WireEvents();
        }

        private void WireEvents()
        {
            _vm.RequestAddProjectDialog += OnAddProject;
            _vm.RequestEditProjectDialog += OnEditProject;
            _vm.RequestAddMilestoneDialog += OnAddMilestone;
            _vm.RequestAddTaskDialog += OnAddTask;
        }

        private void OnAddProject(ProjectModel _, List<ClientModel> allClients, List<ClientModel> __)
        {
            var dialog = new ProjectDialog(null, allClients, new List<ClientModel>())
            {
                Owner = Window.GetWindow(this)
            };
            if (dialog.ShowDialog() == true)
            {
                _vm.ConfirmAddProject(
                    dialog.ProjectName,
                    dialog.ProjectDescription,
                    dialog.StartDate,
                    dialog.EndDate,
                    dialog.Status,
                    dialog.SelectedClientIds);
            }
        }

        private void OnEditProject(ProjectModel project, List<ClientModel> allClients, List<ClientModel> linked)
        {
            var dialog = new ProjectDialog(project, allClients, linked)
            {
                Owner = Window.GetWindow(this)
            };
            if (dialog.ShowDialog() == true)
            {
                var updated = new ProjectModel
                {
                    Id = project.Id,
                    Name = dialog.ProjectName,
                    Description = dialog.ProjectDescription,
                    StartDate = System.DateTime.TryParse(dialog.StartDate, out var sd) ? sd : System.DateTime.MinValue,
                    EndDate = System.DateTime.TryParse(dialog.EndDate, out var ed) ? ed : System.DateTime.MinValue,
                    Status = dialog.Status
                };

                var previousIds = linked.Select(c => c.Id).ToList();
                var newIds = dialog.SelectedClientIds.Except(previousIds).ToList();
                var removedIds = previousIds.Except(dialog.SelectedClientIds).ToList();

                _vm.ConfirmEditProject(updated, newIds, removedIds);
            }
        }

        private void OnAddMilestone(int projectId)
        {
            var dialog = new MilestoneDialog { Owner = Window.GetWindow(this) };
            if (dialog.ShowDialog() == true)
            {
                _vm.ConfirmAddMilestone(projectId, dialog.MilestoneDescription, dialog.Deadline);
            }
        }

        private void OnAddTask(int milestoneId)
        {
            var dialog = new TaskDialog { Owner = Window.GetWindow(this) };
            if (dialog.ShowDialog() == true)
            {
                _vm.ConfirmAddTask(milestoneId, dialog.TaskName, dialog.TaskDescription, dialog.Deadline);
            }
        }
    }
}