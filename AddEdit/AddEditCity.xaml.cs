#nullable enable
using Businfo.Helper;
using Businfo.Manage;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Businfo.AddEdit
{
    public partial class AddEditCity : Window
    {
        private readonly bool isNew;
        private string existingId;
        private string currentGuid;

        public ObservableCollection<BusData> Matched { get; } = new ObservableCollection<BusData>();

        // 1. New City Constructor
        public AddEditCity()
        {
            InitializeComponent();
            isNew = true;
            existingId = "0";
            currentGuid = Guid.NewGuid().ToString();

            matched_grid.ItemsSource = Matched;

            txtCity.TextChanged += (s, e) => FilterMatchedBuses(txtCity.Text?.Trim() ?? string.Empty);

            LoadBusesFromCache();

#if DEBUG
            FillDummyData();
#endif
        }

        // 2. Direct Model Overload (Fast & 0 Network Call) ⚡
        public AddEditCity(CityModel city)
        {
            InitializeComponent();
            isNew = false;
            existingId = city.DocId ?? "0";
            currentGuid = !string.IsNullOrWhiteSpace(city.GUID) ? city.GUID : Guid.NewGuid().ToString();

            matched_grid.ItemsSource = Matched;

            txtCity.TextChanged += (s, e) => FilterMatchedBuses(txtCity.Text?.Trim() ?? string.Empty);

            PopulateCityFields(city);
            LoadBusesFromCache();
        }

        // 3. Fallback ID Overload (Sirf jab model available na ho)
        public AddEditCity(string id)
        {
            InitializeComponent();
            isNew = false;
            existingId = id;
            currentGuid = Guid.NewGuid().ToString();

            matched_grid.ItemsSource = Matched;

            txtCity.TextChanged += (s, e) => FilterMatchedBuses(txtCity.Text?.Trim() ?? string.Empty);

            LoadBusesFromCache();
            LoadCityByIdAsync(id);
        }

#if DEBUG
        private void FillDummyData()
        {
            int rnd = Random.Shared.Next(10, 99);
            txtState.Text = "Rajasthan";
            txtDistrict.Text = "Jaipur";
            txtCity.Text = $"Ajmer {rnd}";
            cityCode.Text = $"AJM{rnd}";
        }
#endif

        private void PopulateCityFields(CityModel city)
        {
            txtCity.Text = city.CityName ?? string.Empty;
            txtDistrict.Text = city.District ?? string.Empty;
            txtState.Text = city.State ?? string.Empty;
            cityCode.Text = city.CityCode ?? string.Empty;
            existingId = city.DocId ?? existingId;
            currentGuid = !string.IsNullOrWhiteSpace(city.GUID) ? city.GUID : currentGuid;

            FilterMatchedBuses(city.CityName ?? string.Empty);
        }

        private async void LoadBusesFromCache()
        {
            await ApiClient.GetCachedBusesAsync();
            FilterMatchedBuses(txtCity.Text?.Trim() ?? string.Empty);
        }

        private async void LoadCityByIdAsync(string id)
        {
            try
            {
                var city = await ApiClient.GetCityByIdAsync(id);
                if (city == null)
                {
                    MessageBox.Show("City data not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                PopulateCityFields(city);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching city details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void FilterMatchedBuses(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
            {
                Matched.Clear();
                return;
            }

            Matched.Clear();

            lock (ApiClient.BusCache)
            {
                foreach (var bus in ApiClient.BusCache)
                {
                    if (bus == null) continue;

                    bool isMatch = string.Equals(bus.From, cityName, StringComparison.OrdinalIgnoreCase) ||
                                   string.Equals(bus.To, cityName, StringComparison.OrdinalIgnoreCase);

                    if (!isMatch && bus.Stops != null)
                    {
                        foreach (var stop in bus.Stops)
                        {
                            if (stop != null && string.Equals(stop.Station, cityName, StringComparison.OrdinalIgnoreCase))
                            {
                                isMatch = true;
                                break;
                            }
                        }
                    }

                    if (isMatch)
                    {
                        Matched.Add(bus);
                    }
                }
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string? cityName = txtCity.Text?.Trim();
            string? district = txtDistrict.Text?.Trim();
            string? state = txtState.Text?.Trim();
            string? code = cityCode.Text?.Trim();

            if (string.IsNullOrWhiteSpace(cityName))
            {
                MessageBox.Show("City Name bharna anivarya hai.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var cityModel = new CityModel
            {
                DocId = isNew ? "0" : existingId,
                CityName = cityName,
                District = district ?? string.Empty,
                State = state ?? string.Empty,
                CityCode = code ?? string.Empty,
                GUID = currentGuid
            };

            var (success, errorMsg) = await ApiClient.SaveCityAsync(cityModel);
            if (success)
            {
                MessageBox.Show(isNew ? "New city added successfully!" : "City updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            else
            {
                MessageBox.Show($"Server par city save nahi ho saki:\n{errorMsg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Discard_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (matched_grid.SelectedItem is BusData bus)
            {
                var addEditBus = new AddEditBus(bus);
                addEditBus.ShowDialog();
                FilterMatchedBuses(txtCity.Text?.Trim() ?? string.Empty);
            }
        }
    }
}