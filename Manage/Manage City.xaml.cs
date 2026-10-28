using Businfo.AddEdit;
using Businfo.Helper;
using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Windows;

namespace Businfo.Manage
{
    public partial class Manage_City : Window
    {
        public ObservableCollection<CityModel> CityList { get; set; } = new ObservableCollection<CityModel>();

        public Manage_City()
        {
            InitializeComponent();
            BusDataGrid.ItemsSource = CityList;
            LoadCities();
        }

        public async void LoadCities()
        {
            try
            {
                var cities = await ApiClient.GetCitiesAsync();
                CityList.Clear();
                if (cities != null)
                {
                    foreach (var city in cities)
                    {
                        CityList.Add(city);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cities load karne me error: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Add New City
        private void Button_Click_Add(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditCity();
            addWindow.ShowDialog();
            LoadCities();
        }

        // Edit City
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (BusDataGrid.SelectedItem is CityModel selectedCity)
            {
                var editWindow = new AddEditCity(selectedCity.DocId);
                editWindow.ShowDialog();
                LoadCities();
            }
        }

        // Delete City
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (BusDataGrid.SelectedItem is CityModel selectedCity)
            {
                var result = MessageBox.Show($"Are you sure you want to delete {selectedCity.CityName}?",
                                             "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    bool success = await ApiClient.DeleteCityAsync(selectedCity.DocId);
                    if (success)
                    {
                        LoadCities();
                    }
                    else
                    {
                        MessageBox.Show("City delete karne me error aaya.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }

    public class CityModel
    {
        [JsonPropertyName("DocId")]
        public string DocId { get; set; }

        [JsonPropertyName("CityName")]
        public string CityName { get; set; }

        [JsonPropertyName("State")]
        public string State { get; set; }

        [JsonPropertyName("District")]
        public string District { get; set; }

        [JsonPropertyName("CityCode")]
        public string CityCode { get; set; }

        [JsonPropertyName("GUID")]
        public string GUID { get; set; }

        public CityModel Clone()
        {
            return new CityModel
            {
                DocId = this.DocId,
                CityName = this.CityName,
                State = this.State,
                District = this.District,
                CityCode = this.CityCode,
                GUID = this.GUID
            };
        }
    }
}