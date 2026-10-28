using Businfo.AddEdit;
using Businfo.Helper;
using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Windows;

namespace Businfo.Manage
{
    public partial class Manage_Depot : Window
    {
        public ObservableCollection<DepotData> Depots { get; set; } = new ObservableCollection<DepotData>();

        public Manage_Depot()
        {
            InitializeComponent();
            dgDepots.ItemsSource = Depots;
            LoadDepots();
        }

        public async void LoadDepots()
        {
            try
            {
                var list = await ApiClient.GetDepotsAsync();
                Depots.Clear();
                if (list != null)
                {
                    foreach (var d in list)
                    {
                        Depots.Add(d);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Depots load karne me error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAddDepot_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditDepot();
            addWindow.ShowDialog();
            LoadDepots();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (dgDepots.SelectedItem is DepotData selectedDepot)
            {
                var editWindow = new AddEditDepot(selectedDepot.Id);
                editWindow.ShowDialog();
                LoadDepots();
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (dgDepots.SelectedItem is DepotData selectedDepot)
            {
                var result = MessageBox.Show($"Are you sure you want to delete {selectedDepot.Depot}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    bool success = await ApiClient.DeleteDepotAsync(selectedDepot.Id);
                    if (success)
                    {
                        LoadDepots();
                    }
                    else
                    {
                        MessageBox.Show("Delete karne me error aaya.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }

    public class DepotData
    {
        [JsonPropertyName("Id")]
        public string Id { get; set; }

        [JsonPropertyName("State")]
        public string State { get; set; }

        [JsonPropertyName("City")]
        public string City { get; set; }

        [JsonPropertyName("Depot")]
        public string Depot { get; set; }

        [JsonPropertyName("DepotNumber")]
        public string DepotNumber { get; set; }

        [JsonPropertyName("GUID")]
        public string GUID { get; set; }

        public DepotData Clone()
        {
            return new DepotData
            {
                Id = this.Id,
                State = this.State,
                City = this.City,
                Depot = this.Depot,
                DepotNumber = this.DepotNumber,
                GUID = this.GUID
            };
        }
    }
}