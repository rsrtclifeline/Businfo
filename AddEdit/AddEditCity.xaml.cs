using Businfo.Helper;
using Businfo.Manage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Businfo.AddEdit
{
    public partial class AddEditCity : Window
    {
        private bool isNew;
        private string existingId;
        private string currentGuid;

        public ObservableCollection<BusData> matched { get; private set; } = new ObservableCollection<BusData>();
        private List<BusData> allBusesCache = new List<BusData>();

        public AddEditCity()
        {
            InitializeComponent();
            isNew = true;
            existingId = "0";
            currentGuid = Guid.NewGuid().ToString();

            matched_grid.ItemsSource = matched;
            LoadAllBusesCacheAsync();

#if DEBUG
            FillDummyData();
#endif
        }

        public AddEditCity(string id)
        {
            InitializeComponent();
            isNew = false;
            existingId = id;
            matched_grid.ItemsSource = matched;

            LoadAllBusesCacheAsync();
            LoadCityByIdAsync(id);
        }

#if DEBUG
        private void FillDummyData()
        {
            var rnd = new Random().Next(10, 99);
            txtState.Text = "Rajasthan";
            txtDistrict.Text = "Jaipur";
            txtCity.Text = $"Ajmer {rnd}";
            cityCode.Text = $"AJM{rnd}";
        }
#endif

        private async void LoadCityByIdAsync(string id)
        {
            try
            {
                var city = await ApiClient.GetCityByIdAsync(id);
                if (city == null)
                {
                    MessageBox.Show("City data not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                txtCity.Text = city.CityName ?? "";
                txtDistrict.Text = city.District ?? "";
                txtState.Text = city.State ?? "";
                cityCode.Text = city.CityCode ?? "";
                currentGuid = !string.IsNullOrWhiteSpace(city.GUID) ? city.GUID : Guid.NewGuid().ToString();

                FilterMatchedBuses(city.CityName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching city details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private async void LoadAllBusesCacheAsync()
        {
            try
            {
                var buses = await ApiClient.GetBusesAsync();
                allBusesCache = buses ?? new List<BusData>();

                if (!string.IsNullOrWhiteSpace(txtCity.Text))
                {
                    FilterMatchedBuses(txtCity.Text.Trim());
                }
            }
            catch { }
        }

        private void FilterMatchedBuses(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
            {
                matched.Clear();
                return;
            }

            matched.Clear();
            foreach (var bus in allBusesCache)
            {
                bool isMatch = false;

                if (string.Equals(bus.From, cityName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(bus.To, cityName, StringComparison.OrdinalIgnoreCase))
                {
                    isMatch = true;
                }

                if (!isMatch && bus.Stops != null)
                {
                    foreach (var stop in bus.Stops)
                    {
                        if (string.Equals(stop.Station, cityName, StringComparison.OrdinalIgnoreCase))
                        {
                            isMatch = true;
                            break;
                        }
                    }
                }

                if (isMatch)
                {
                    matched.Add(bus);
                }
            }
        }

        // Save
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var cityName = txtCity.Text?.Trim();
            var district = txtDistrict.Text?.Trim();
            var state = txtState.Text?.Trim();
            var code = cityCode.Text?.Trim();

            if (string.IsNullOrWhiteSpace(cityName))
            {
                MessageBox.Show("City Name bharna anivarya hai.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var cityModel = new CityModel
            {
                DocId = isNew ? "0" : existingId,
                CityName = cityName,
                District = district ?? "",
                State = state ?? "",
                CityCode = code ?? "",
                GUID = currentGuid
            };

            var (success, errorMsg) = await ApiClient.SaveCityAsync(cityModel);
            if (success)
            {
                MessageBox.Show(isNew ? "New city added successfully!" : "City updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show($"Server par city save nahi ho saki:\n{errorMsg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Discard_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Open Selected Bus from Matched Grid
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (matched_grid.SelectedItem is BusData bus)
            {
                AddEditBus addEditBus = new AddEditBus(bus);
                addEditBus.ShowDialog();
            }
        }
    }
}