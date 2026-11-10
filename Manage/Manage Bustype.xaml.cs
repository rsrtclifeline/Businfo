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
    public partial class Manage_Bustype : Window
    {
        public ObservableCollection<BusTypeModel> BusTypes { get; set; } = new ObservableCollection<BusTypeModel>();

        public Manage_Bustype()
        {
            InitializeComponent();
            dgBusTypes.ItemsSource = BusTypes;
            this.Closed += Manage_Bustype_Closed;

            // Initial API load
            LoadBusTypes();

            // Realtime socket events setup
            SetupSocketListeners();
        }

        public async void LoadBusTypes()
        {
            try
            {
                var list = await Task.Run(async () => await ApiClient.GetBusTypesAsync());
                if (list != null && !Dispatcher.HasShutdownStarted)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        BusTypes.Clear();
                        foreach (var item in list)
                        {
                            BusTypes.Add(item);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bus Types load karne me error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetupSocketListeners()
        {
            if (ApiClient.Socket == null) return;

            // 1. Live BusType Created
            ApiClient.Socket.On("bustype_created", async context =>
            {
                try
                {
                    if (context == null || Dispatcher.HasShutdownStarted) return;
                    var newType = context.GetValue<BusTypeModel>(0);

                    if (newType != null)
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            if (!BusTypes.Any(b => b.id == newType.id))
                            {
                                BusTypes.Insert(0, newType);
                            }
                        });
                    }
                }
                catch { }
            });

            // 2. Live BusType Updated
            ApiClient.Socket.On("bustype_updated", async context =>
            {
                try
                {
                    if (context == null || Dispatcher.HasShutdownStarted) return;
                    var updatedType = context.GetValue<BusTypeModel>(0);

                    if (updatedType != null)
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            var existing = BusTypes.FirstOrDefault(b => b.id == updatedType.id);
                            if (existing != null)
                            {
                                int idx = BusTypes.IndexOf(existing);
                                BusTypes[idx] = updatedType;
                            }
                            else
                            {
                                BusTypes.Insert(0, updatedType);
                            }
                        });
                    }
                }
                catch { }
            });

            // 3. Live BusType Deleted
            ApiClient.Socket.On("bustype_deleted", async context =>
            {
                try
                {
                    if (context == null || Dispatcher.HasShutdownStarted) return;
                    var payload = context.GetValue<BusTypeDeletePayload>(0);

                    if (payload != null && !string.IsNullOrWhiteSpace(payload.id))
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            var item = BusTypes.FirstOrDefault(b => b.id == payload.id);
                            if (item != null)
                            {
                                BusTypes.Remove(item);
                            }
                        });
                    }
                }
                catch { }
            });
        }

        // Add New (No reload required, socket adds automatically)
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditBusType();
            addWindow.ShowDialog();
        }

        // Edit (Direct clone pass - 0ms open, zero HTTP calls)
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgBusTypes.SelectedItem is BusTypeModel m)
            {
                var editWindow = new AddEditBusType(m.Clone());
                editWindow.ShowDialog();
            }
        }

        // Delete (No manual reload, socket handles removal)
        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgBusTypes.SelectedItem is BusTypeModel m)
            {
                var result = MessageBox.Show($"Are you sure you want to delete {m.BusType}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    bool success = await ApiClient.DeleteBusTypeAsync(m.id);
                    if (!success)
                    {
                        MessageBox.Show("Delete karne me error aaya.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Manage_Bustype_Closed(object? sender, EventArgs e)
        {
            try
            {
                ApiClient.Socket?.On("bustype_created", _ => Task.CompletedTask);
                ApiClient.Socket?.On("bustype_updated", _ => Task.CompletedTask);
                ApiClient.Socket?.On("bustype_deleted", _ => Task.CompletedTask);
            }
            catch { }
        }
    }

    public class BusTypeModel
    {
        [JsonPropertyName("id")]
        public string id { get; set; } = string.Empty;

        [JsonPropertyName("BusType")]
        public string BusType { get; set; } = string.Empty;

        // Fallback mapping in case backend sends typeName
        [JsonPropertyName("typeName")]
        public string? TypeNameFallback
        {
            get => BusType;
            set { if (string.IsNullOrWhiteSpace(BusType) && !string.IsNullOrWhiteSpace(value)) BusType = value; }
        }

        [JsonPropertyName("GUID")]
        public string GUID { get; set; } = string.Empty;

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

    public class BusTypeDeletePayload
    {
        [JsonPropertyName("id")]
        public string id { get; set; } = string.Empty;
    }
}