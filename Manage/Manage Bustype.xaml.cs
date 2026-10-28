using Businfo.AddEdit;
using Businfo.Helper;
using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Windows;

namespace Businfo.Manage
{
    public partial class Manage_Bustype : Window
    {
        public ObservableCollection<BusTypeModel> BusTypes { get; set; } = new ObservableCollection<BusTypeModel>();

        public Manage_Bustype()
        {
            InitializeComponent();
            dgBusTypes.ItemsSource = BusTypes;
            LoadBusTypes();
        }

        public async void LoadBusTypes()
        {
            try
            {
                var list = await ApiClient.GetBusTypesAsync();
                BusTypes.Clear();
                if (list != null)
                {
                    foreach (var item in list)
                    {
                        BusTypes.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bus Types load karne me error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditBusType();
            addWindow.ShowDialog();
            LoadBusTypes();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgBusTypes.SelectedItem is BusTypeModel m)
            {
                var editWindow = new AddEditBusType(m.id);
                editWindow.ShowDialog();
                LoadBusTypes();
            }
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgBusTypes.SelectedItem is BusTypeModel m)
            {
                var result = MessageBox.Show($"Are you sure you want to delete {m.BusType}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    bool success = await ApiClient.DeleteBusTypeAsync(m.id);
                    if (success)
                    {
                        LoadBusTypes();
                    }
                    else
                    {
                        MessageBox.Show("Delete karne me error aaya.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }

    public class BusTypeModel
    {
        [JsonPropertyName("id")]
        public string id { get; set; }

        [JsonPropertyName("BusType")]
        public string BusType { get; set; }

        [JsonPropertyName("GUID")]
        public string GUID { get; set; }

        public BusTypeModel Clone()
        {
            return new BusTypeModel
            {
                GUID = this.GUID,
                BusType = this.BusType,
                id = this.id
            };
        }
    }
}