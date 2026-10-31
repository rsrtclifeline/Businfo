using Businfo.AddEdit;
using Businfo.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Businfo.Manage
{
    public partial class Manage_Bus : Window
    {
        public ObservableCollection<BusData> DisplayedBuses { get; set; } = new ObservableCollection<BusData>();
        private List<BusData> masterBusList = new List<BusData>();
        private bool isInitialized = false;

        public Manage_Bus()
        {
            InitializeComponent();
            BusDataGrid.ItemsSource = DisplayedBuses;

            isInitialized = true; // Controls ready hone ke baad hi filtering allow hogi

            LoadVendorsDropdown();
            LoadBusData();
        }

        public async void LoadBusData()
        {
            try
            {
                var buses = await ApiClient.GetBusesAsync();
                masterBusList = buses ?? new List<BusData>();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Server se data load karne me error: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadVendorsDropdown()
        {
            try
            {
                var meta = await ApiClient.GetMetadataAsync();
                var vendors = new List<string> { "All Vendors" };
                if (meta?.vendors != null)
                {
                    vendors.AddRange(meta.vendors);
                }

                cmbVendorFilter.ItemsSource = vendors;
                cmbVendorFilter.SelectedIndex = 0;
            }
            catch { }
        }

        private void ApplyFilters()
        {
            // Null Guard: Agar controls ya list abhi instantiate nahi hue toh aage mat badho
            if (!isInitialized || SearchTextBox == null || cmbVendorFilter == null || masterBusList == null)
                return;

            var searchTxt = SearchTextBox.Text?.Trim().ToLower() ?? "";
            if (searchTxt == "search route, city, bus...")
                searchTxt = "";

            var selectedVendor = cmbVendorFilter.SelectedItem?.ToString();

            var filtered = masterBusList.Where(b =>
            {
                if (b == null) return false;

                // 1. Vendor Filter Check
                bool matchVendor = string.IsNullOrEmpty(selectedVendor) ||
                                   selectedVendor == "All Vendors" ||
                                   string.Equals(b.Vendor, selectedVendor, StringComparison.OrdinalIgnoreCase);

                if (!matchVendor) return false;

                // 2. Search Box Filter Check
                if (string.IsNullOrEmpty(searchTxt)) return true;

                return (!string.IsNullOrEmpty(b.Line) && b.Line.ToLower().Contains(searchTxt)) ||
                       (!string.IsNullOrEmpty(b.From) && b.From.ToLower().Contains(searchTxt)) ||
                       (!string.IsNullOrEmpty(b.To) && b.To.ToLower().Contains(searchTxt)) ||
                       (!string.IsNullOrEmpty(b.Route) && b.Route.ToLower().Contains(searchTxt)) ||
                       (!string.IsNullOrEmpty(b.Depot) && b.Depot.ToLower().Contains(searchTxt)) ||
                       (!string.IsNullOrEmpty(b.Vendor) && b.Vendor.ToLower().Contains(searchTxt));
            }).ToList();

            DisplayedBuses.Clear();
            foreach (var b in filtered)
            {
                DisplayedBuses.Add(b);
            }

            if (txtRecordCount != null)
            {
                txtRecordCount.Text = $"Showing {DisplayedBuses.Count} of {masterBusList.Count} routes";
            }
        }

        // Search Handlers
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "Search route, city, bus...")
            {
                SearchTextBox.Text = "";
                SearchTextBox.Foreground = Brushes.White;
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = "Search route, city, bus...";
                SearchTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8E92BC"));
            }
        }

        private void cmbVendorFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        // Add Bus
        private void btnAddBus_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditBus();
            addWindow.ShowDialog();
            LoadBusData();
        }

        // Edit Bus
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (BusDataGrid.SelectedItem is BusData a)
            {
                var s = new AddEditBus(a.id);
                s.ShowDialog();
                LoadBusData();
            }
        }

        // Clone Bus
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (BusDataGrid.SelectedItem is BusData s)
            {
                var cloned = s.Clone();
                var a = new AddEditBus(cloned);
                a.ShowDialog();
                LoadBusData();
            }
        }

        // Delete Bus
        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (BusDataGrid.SelectedItem is BusData s)
            {
                var result = MessageBox.Show($"Route '{s.Line}' ({s.From} -> {s.To}) delete karein?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    bool success = await ApiClient.DeleteBusAsync(s.id);
                    if (success)
                    {
                        LoadBusData();
                    }
                    else
                    {
                        MessageBox.Show("Bus delete karne me error aaya.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
  // Import Excel Button (Temporary)
private void btnImportExcel_Click(object sender, RoutedEventArgs e)
{
    MessageBox.Show(
        "Excel Import feature will be available soon!",
        "RSRTC LifeLine",
        MessageBoxButton.OK,
        MessageBoxImage.Information
    );
}  }
}
