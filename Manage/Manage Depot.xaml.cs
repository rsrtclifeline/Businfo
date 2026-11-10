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

namespace Businfo.Manage
{
    public partial class Manage_Depot : Window
    {
        public ObservableCollection<DepotData> Depots { get; set; } = new ObservableCollection<DepotData>();

        public Manage_Depot()
        {
            InitializeComponent();
            dgDepots.ItemsSource = Depots;
            this.Closed += Manage_Depot_Closed;

            // Initial REST API load
            LoadDepots();

            // Realtime background sync
            SetupSocketListeners();
        }

        public async void LoadDepots()
        {
            try
            {
                var list = await Task.Run(async () => await ApiClient.GetDepotsAsync());
                if (list != null && !Dispatcher.HasShutdownStarted)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        Depots.Clear();
                        foreach (var d in list)
                        {
                            Depots.Add(d);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Depots load karne me error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetupSocketListeners()
        {
            if (ApiClient.Socket == null) return;

            // 1. Live Depot Created
            ApiClient.Socket.On("depot_created", async context =>
            {
                try
                {
                    if (context == null || Dispatcher.HasShutdownStarted) return;
                    var newDepot = context.GetValue<DepotData>(0);

                    if (newDepot != null)
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            // GUID match check (agar GUID empty ho toh Id check)
                            bool exists = Depots.Any(d =>
                                (!string.IsNullOrWhiteSpace(d.GUID) && d.GUID == newDepot.GUID) ||
                                (d.Id != "0" && d.Id == newDepot.Id));

                            if (!exists)
                            {
                                Depots.Insert(0, newDepot);
                            }
                        });
                    }
                }
                catch { }
            });

            // 2. Live Depot Updated
            ApiClient.Socket.On("depot_updated", async context =>
            {
                try
                {
                    if (context == null || Dispatcher.HasShutdownStarted) return;
                    var updatedDepot = context.GetValue<DepotData>(0);

                    if (updatedDepot != null)
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            var existing = Depots.FirstOrDefault(d =>
                                (!string.IsNullOrWhiteSpace(d.GUID) && d.GUID == updatedDepot.GUID) ||
                                (d.Id != "0" && d.Id == updatedDepot.Id));

                            if (existing != null)
                            {
                                int idx = Depots.IndexOf(existing);
                                Depots[idx] = updatedDepot;
                            }
                            else
                            {
                                Depots.Insert(0, updatedDepot);
                            }
                        });
                    }
                }
                catch { }
            });

            // 3. Live Depot Deleted
            ApiClient.Socket.On("depot_deleted", async context =>
            {
                try
                {
                    if (context == null || Dispatcher.HasShutdownStarted) return;
                    var payload = context.GetValue<DepotDeletePayload>(0);

                    if (payload != null)
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            var item = Depots.FirstOrDefault(d =>
                                (!string.IsNullOrWhiteSpace(payload.GUID) && d.GUID == payload.GUID) ||
                                (!string.IsNullOrWhiteSpace(payload.Id) && d.Id == payload.Id));

                            if (item != null)
                            {
                                Depots.Remove(item);
                            }
                        });
                    }
                }
                catch { }
            });
        }

        // Add New Depot
        private void btnAddDepot_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditDepot();
            addWindow.ShowDialog();
        }

        // Edit Depot (Direct Clone Injection - 0ms open, zero HTTP requests)
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (dgDepots.SelectedItem is DepotData selectedDepot)
            {
                var editWindow = new AddEditDepot(selectedDepot.Clone());
                editWindow.ShowDialog();
            }
        }

        // Delete Depot
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (dgDepots.SelectedItem is DepotData selectedDepot)
            {
                var result = MessageBox.Show($"Are you sure you want to delete {selectedDepot.Depot}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    // Target Id agar valid ho toh Id, warna GUID bhejo
                    string deleteKey = selectedDepot.Id != "0" && !string.IsNullOrWhiteSpace(selectedDepot.Id)
                        ? selectedDepot.Id
                        : selectedDepot.GUID;

                    bool success = await ApiClient.DeleteDepotAsync(deleteKey);
                    if (!success)
                    {
                        MessageBox.Show("Delete karne me error aaya.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Manage_Depot_Closed(object? sender, EventArgs e)
        {
            try
            {
                ApiClient.Socket?.On("depot_created", _ => Task.CompletedTask);
                ApiClient.Socket?.On("depot_updated", _ => Task.CompletedTask);
                ApiClient.Socket?.On("depot_deleted", _ => Task.CompletedTask);
            }
            catch { }
        }
    }

    public class DepotData
    {
        [JsonPropertyName("Id")]
        public string Id { get; set; } = "0";

        [JsonPropertyName("State")]
        public string State { get; set; } = string.Empty;

        [JsonPropertyName("City")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("Depot")]
        public string Depot { get; set; } = string.Empty;

        // Metadata dropdowns ke sath name compatibility fallback
        [JsonPropertyName("DepotName")]
        public string? DepotNameFallback
        {
            get => Depot;
            set { if (string.IsNullOrWhiteSpace(Depot) && !string.IsNullOrWhiteSpace(value)) Depot = value; }
        }

        [JsonPropertyName("DepotNumber")]
        public string DepotNumber { get; set; } = string.Empty;

        [JsonPropertyName("GUID")]
        public string GUID { get; set; } = string.Empty;

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

    public class DepotDeletePayload
    {
        [JsonPropertyName("Id")]
        public string? Id { get; set; }

        [JsonPropertyName("GUID")]
        public string? GUID { get; set; }
    }
}