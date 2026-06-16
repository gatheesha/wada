using System;
using System.Windows;
using System.Windows.Controls;
using wada.Data;
using wada.Models;

namespace wada.Views
{
  
    public partial class EarningsWindow : Window
    {
        private readonly DatabaseContext _dbContext;
        private EarningModel _selectedEarning;
        public EarningsWindow()
        {
            InitializeComponent();
            _dbContext = new DatabaseContext();
            LoadDropdownData(); // populate client/project combo boxes
            LoadData();
            DpDate.SelectedDate = DateTime.Today;
        }

        private void LoadData()
        {
            DGEarnings.ItemsSource = _dbContext.GetAllEarnings();
        }

        private void LoadDropdownData()
        {
            // Fetch records to bind directly to our combo selectors
            var clients = _dbContext.GetAllClients();
            var projects = _dbContext.GetAllProjects();

            // Insert placeholder default values at index 0 
            clients.Insert(0, new ClientModel { Id = 0, Name = "-- None --" });
            projects.Insert(0, new ProjectModel { Id = 0, Name = "-- None --" });

            CboClient.ItemsSource = clients;
            CboProject.ItemsSource = projects;

            CboClient.SelectedIndex = 0;
            CboProject.SelectedIndex = 0;
        }

        private void FilterInputs_Changed(object sender, RoutedEventArgs e)
        {
            if (_dbContext == null || TxtSearch == null || CboFilterType == null) return;

            string searchText = TxtSearch.Text;
            string typeFilter = (CboFilterType.SelectedItem as ComboBoxItem)?.Content.ToString();

            DGEarnings.ItemsSource = _dbContext.FilterEarnings(searchText, typeFilter);
        }

        private void DGEarnings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedEarning = DGEarnings.SelectedItem as EarningModel;
            if (_selectedEarning != null)
            {
                TxtAmount.Text = _selectedEarning.Amount.ToString();
                DpDate.SelectedDate = _selectedEarning.Date;
                TxtDescription.Text = _selectedEarning.Description;
                CboFormType.Text = _selectedEarning.Type;

                // Sync selected combo ids
                CboClient.SelectedValue = _selectedEarning.ClientId ?? 0;
                CboProject.SelectedValue = _selectedEarning.ProjectId ?? 0;
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(TxtAmount.Text, out double amount))
            {
                MessageBox.Show("Please enter a valid numeric value for the transaction amount.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string type = (CboFormType.SelectedItem as ComboBoxItem).Content.ToString();
            string date = DpDate.SelectedDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");
            string desc = TxtDescription.Text;

            // Extract IDs (0 means fallback to DB null)
            int? selectedClientId = (int?)CboClient.SelectedValue == 0 ? null : (int?)CboClient.SelectedValue;
            int? selectedProjectId = (int?)CboProject.SelectedValue == 0 ? null : (int?)CboProject.SelectedValue;

            _dbContext.AddEarning(amount, date, type, desc, selectedProjectId, selectedClientId);
            LoadData();
            ClearForm();
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedEarning == null)
            {
                MessageBox.Show("Please select an earning row from the grid list to update.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!double.TryParse(TxtAmount.Text, out double amount))
            {
                MessageBox.Show("Please enter a valid amount numerical value.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _selectedEarning.Amount = amount;
            _selectedEarning.Type = (CboFormType.SelectedItem as ComboBoxItem).Content.ToString();
            _selectedEarning.Date = DpDate.SelectedDate ?? DateTime.Today;
            _selectedEarning.Description = TxtDescription.Text;

            _selectedEarning.ClientId = (int?)CboClient.SelectedValue == 0 ? null : (int?)CboClient.SelectedValue;
            _selectedEarning.ProjectId = (int?)CboProject.SelectedValue == 0 ? null : (int?)CboProject.SelectedValue;

            _dbContext.UpdateEarning(_selectedEarning);
            LoadData();
            ClearForm();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedEarning == null)
            {
                MessageBox.Show("Please select a transaction record row to delete.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show("Are you sure you want to permanently delete this item?", "Confirm Action", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                _dbContext.DeleteEarning(_selectedEarning.Id);
                LoadData();
                ClearForm();
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            DGEarnings.SelectedItem = null;
            _selectedEarning = null;
            TxtAmount.Clear();
            TxtDescription.Clear();
            DpDate.SelectedDate = DateTime.Today;
            CboFormType.SelectedIndex = 0;
            CboClient.SelectedIndex = 0;
            CboProject.SelectedIndex = 0;
        }
    }
}