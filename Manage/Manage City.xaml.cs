#nullable enable
using Businfo.AddEdit;
using Businfo.Helper;
using SocketIOClient;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Businfo.Manage
{
    public partial class Manage_City : Window
    {
        public ObservableCollection<CityModel> CityList { get; set; } = new ObservableCollection<CityModel>();

        public Manage_City()
        {
            InitializeComponent();
            BusDataGrid.ItemsSource = CityList;
            this.Closed += Manage_City_Closed;

            // Pehli baar REST API se load karega
            LoadCities();

            // Uske baad poora sync realtime socket events se hoga
            SetupSocketListeners();
        }

        public async void LoadCities()
        {
            try
            {
                var cities = await Task.Run(async () => await ApiClient.GetCitiesAsync());

                if (cities != null && !Dispatcher.HasShutdownStarted)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        CityList.Clear();
                        foreach (var city in cities)
                        {
                            CityList.Add(city);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cities load karne me error: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetupSocketListeners()
        {
            if (ApiClient.Socket == null) return;

            // 1. Live City Added
            ApiClient.Socket.On("city_created", async context =>
            {
                try
                {
                    if (context == null || Dispatcher.HasShutdownStarted) return;
                    var newCity = context.GetValue<CityModel>(0);

                    if (newCity != null)
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            if (!CityList.Any(c => c.DocId == newCity.DocId))
                            {
                                CityList.Insert(0, newCity);
                            }
                        });
                    }
                }
                catch { }
            });

            // 2. Live City Updated
            ApiClient.Socket.On("city_updated", async context =>
            {
                try
                {
                    if (context == null || Dispatcher.HasShutdownStarted) return;
                    var updatedCity = context.GetValue<CityModel>(0);

                    if (updatedCity != null)
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            var existing = CityList.FirstOrDefault(c => c.DocId == updatedCity.DocId);
                            if (existing != null)
                            {
                                int idx = CityList.IndexOf(existing);
                                CityList[idx] = updatedCity;
                            }
                            else
                            {
                                CityList.Insert(0, updatedCity);
                            }
                        });
                    }
                }
                catch { }
            });

            // 3. Live City Deleted
            ApiClient.Socket.On("city_deleted", async context =>
            {
                try
                {
                    if (context == null || Dispatcher.HasShutdownStarted) return;
                    var payload = context.GetValue<CityDeletePayload>(0);

                    if (payload != null && !string.IsNullOrWhiteSpace(payload.DocId))
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            var item = CityList.FirstOrDefault(c => c.DocId == payload.DocId);
                            if (item != null)
                            {
                                CityList.Remove(item);
                            }
                        });
                    }
                }
                catch { }
            });
        }

        private void TxtCitySearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var filterText = txtCitySearch.Text?.Trim().ToLowerInvariant() ?? string.Empty;
            var view = CollectionViewSource.GetDefaultView(CityList);

            if (view == null) return;

            if (string.IsNullOrWhiteSpace(filterText))
            {
                view.Filter = null;
            }
            else
            {
                view.Filter = obj =>
                {
                    if (obj is CityModel c)
                    {
                        return (!string.IsNullOrEmpty(c.CityName) && c.CityName.ToLowerInvariant().Contains(filterText)) ||
                               (!string.IsNullOrEmpty(c.CityCode) && c.CityCode.ToLowerInvariant().Contains(filterText)) ||
                               (!string.IsNullOrEmpty(c.District) && c.District.ToLowerInvariant().Contains(filterText));
                    }
                    return false;
                };
            }
        }

        // Add New City (Dialog close par reload ki zaroorat nahi, socket add karega)
        private void Button_Click_Add(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditCity();
            addWindow.ShowDialog();
        }

        // Edit City (Direct model clone pass kiya - 0ms launch & no network call)
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (BusDataGrid.SelectedItem is CityModel selectedCity)
            {
                var editWindow = new AddEditCity(selectedCity.Clone());
                editWindow.ShowDialog();
            }
        }

        // Delete City (Manual reload removed, socket list se uda dega)
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (BusDataGrid.SelectedItem is CityModel selectedCity)
            {
                var result = MessageBox.Show($"Are you sure you want to delete {selectedCity.CityName}?",
                                             "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    bool success = await ApiClient.DeleteCityAsync(selectedCity.DocId);
                    if (!success)
                    {
                        MessageBox.Show("City delete karne me error aaya.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Manage_City_Closed(object? sender, EventArgs e)
        {
            // Socket listeners ko mute/unhook karo taaki dead window crash na kare
            try
            {
                ApiClient.Socket?.On("city_created", _ => Task.CompletedTask);
                ApiClient.Socket?.On("city_updated", _ => Task.CompletedTask);
                ApiClient.Socket?.On("city_deleted", _ => Task.CompletedTask);
            }
            catch { }
        }
    }

    public class CityModel
    {
        [JsonPropertyName("DocId")]
        public string DocId { get; set; } = string.Empty;

        [JsonPropertyName("CityName")]
        public string CityName { get; set; } = string.Empty;

        [JsonPropertyName("State")]
        public string State { get; set; } = string.Empty;

        [JsonPropertyName("District")]
        public string District { get; set; } = string.Empty;

        [JsonPropertyName("CityCode")]
        public string CityCode { get; set; } = string.Empty;

        [JsonPropertyName("GUID")]
        public string GUID { get; set; } = string.Empty;

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

    public class CityDeletePayload
    {
        [JsonPropertyName("DocId")]
        public string DocId { get; set; } = string.Empty;
    }
}