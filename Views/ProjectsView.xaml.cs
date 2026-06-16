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
                // Send string fields straight into our working engine
                _vm.ConfirmAddProject(
                    dialog.ProjectName,
                    dialog.ProjectDescription,
                    dialog.StartDate,     // String from Calendar DatePicker selection 
                    dialog.StartTime,     // String text input box (e.g. "09:00")
                    dialog.DurationDays,  // Integer number of days 
                    dialog.Status,
                    dialog.SelectedClientIds
                );
            }
        }

        private void OnEditProject(ProjectModel project, List<ClientModel> allClients, List<ClientModel> linked)
        {
            var dialog = new ProjectDialog(project, allClients, linked) { Owner = Window.GetWindow(this) };
            if (dialog.ShowDialog() == true)
            {
                project.Name = dialog.ProjectName;
                project.Description = dialog.ProjectDescription;
                project.StartDate = DateTime.Parse(dialog.StartDate);
                project.StartTime = dialog.StartTime;
                project.DurationDays = dialog.DurationDays;
                project.Status = dialog.Status;

                var previousIds = linked.Select(c => c.Id).ToList();
                var newIds = dialog.SelectedClientIds.Except(previousIds).ToList();
                var removedIds = previousIds.Except(dialog.SelectedClientIds).ToList();

                _vm.ConfirmEditProject(project, newIds, removedIds);
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