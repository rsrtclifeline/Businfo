using Businfo.Helper;
using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Businfo.AddEdit
{
    public partial class AddEditStop : Window
    {
        public ObservableCollection<string> Cities { get; set; } = new ObservableCollection<string>();

        private StopData editingData;
        private int editingIndex = -1;
        public Action<int, StopData> Result;

        public AddEditStop()
        {
            InitializeComponent();
            cmbStation.ItemsSource = Cities;
            LoadCities();
        }

        public AddEditStop(StopData data, int index) : this()
        {
            this.editingData = data;
            this.editingIndex = index;
            LoadEditData();
        }

        private async void LoadCities()
        {
            try
            {
                var cityList = await ApiClient.GetCitiesAsync();
                Cities.Clear();
                if (cityList != null)
                {
                    foreach (var c in cityList)
                    {
                        if (!string.IsNullOrWhiteSpace(c.CityName) && !Cities.Contains(c.CityName))
                        {
                            Cities.Add(c.CityName);
                        }
                    }
                }
            }
            catch { }
        }

        private void LoadEditData()
        {
            if (editingData != null)
            {
                cmbStation.Text = editingData.Station;
                txtArrival.Text = editingData.Arrival;
                txtDeparture.Text = editingData.Departure;
                txtDistance.Text = editingData.Distance;
                txtDay.Text = editingData.Day;
            }
        }

        private void ComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (sender is ComboBox combo && combo.ItemsSource != null)
            {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(combo.ItemsSource);
                view.Filter = o =>
                {
                    if (o is string s)
                        return s.IndexOf(combo.Text, StringComparison.OrdinalIgnoreCase) >= 0;
                    return false;
                };
                combo.IsDropDownOpen = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnAddStop_Click(object sender, RoutedEventArgs e)
        {
            var stationName = cmbStation.SelectedItem?.ToString() ?? cmbStation.Text;

            if (string.IsNullOrWhiteSpace(stationName) ||
                string.IsNullOrWhiteSpace(txtArrival.Text) ||
                string.IsNullOrWhiteSpace(txtDeparture.Text) ||
                string.IsNullOrWhiteSpace(txtDistance.Text) ||
                string.IsNullOrWhiteSpace(txtDay.Text))
            {
                MessageBox.Show("Please fill all fields before saving.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var data = new StopData
            {
                Station = stationName.Trim(),
                Arrival = txtArrival.Text.Trim(),
                Departure = txtDeparture.Text.Trim(),
                Distance = txtDistance.Text.Trim(),
                Day = txtDay.Text.Trim(),
                GUID = editingData?.GUID ?? Guid.NewGuid().ToString()
            };

            Result?.Invoke(editingIndex, data);
            this.Close();
        }

        private void btnResetStop_Click(object sender, RoutedEventArgs e)
        {
            cmbStation.Text = "";
            txtArrival.Text = "";
            txtDeparture.Text = "";
            txtDistance.Text = "";
            txtDay.Text = "";
        }
    }

    public class StopData
    {
        [JsonPropertyName("Station")]
        public string Station { get; set; }

        [JsonPropertyName("Arrival")]
        public string Arrival { get; set; }

        [JsonPropertyName("Departure")]
        public string Departure { get; set; }

        [JsonPropertyName("Distance")]
        public string Distance { get; set; }

        [JsonPropertyName("Day")]
        public string Day { get; set; }

        [JsonPropertyName("GUID")]
        public string GUID { get; set; }

        public StopData Clone()
        {
            return new StopData
            {
                Station = this.Station,
                Arrival = this.Arrival,
                Departure = this.Departure,
                Distance = this.Distance,
                Day = this.Day,
                GUID = this.GUID
            };
        }
    }
}