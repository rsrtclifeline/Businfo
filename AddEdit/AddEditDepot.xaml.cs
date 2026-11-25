#nullable enable
using Businfo.Helper;
using Businfo.Manage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Businfo.AddEdit
{
    public partial class AddEditDepot : Window
    {
        private string id = "0";
        private readonly bool isNew = true;
        private string currentGuid = string.Empty;

        // Lightweight ViewModel for Reference List
        private sealed class DepotBusRefItem
        {
            public string BusName { get; set; } = string.Empty;
            public string BusNumber { get; set; } = string.Empty;
            public string RouteSummary { get; set; } = string.Empty;
        }

        // 1. Add New Constructor
        public AddEditDepot()
        {
            InitializeComponent();
            isNew = true;
            id = "0";
            currentGuid = Guid.NewGuid().ToString();
            pnlBusesReference.Visibility = Visibility.Collapsed;

#if DEBUG
            FillDummyData();
#endif
        }

        // 2. Direct Model Injection Constructor (0ms launch, zero network lag)
        public AddEditDepot(DepotData depot)
        {
            InitializeComponent();
            isNew = false;
            id = depot.Id ?? "0";
            currentGuid = !string.IsNullOrWhiteSpace(depot.GUID) ? depot.GUID : Guid.NewGuid().ToString();

            txtState.Text = depot.State ?? string.Empty;
            txtCity.Text = depot.City ?? string.Empty;
            txtDepot.Text = depot.Depot ?? string.Empty;
            txtDepotNumber.Text = depot.DepotNumber ?? string.Empty;

            pnlBusesReference.Visibility = Visibility.Visible;
            _ = LoadDepotBusesAsync(depot.GUID, depot.Depot, depot.DepotNumber);
        }

        // 3. Fallback Key/Id Constructor (Sirf tab chalega agar direct model na ho)
        public AddEditDepot(string key)
        {
            InitializeComponent();
            this.id = key;
            isNew = false;
            currentGuid = Guid.NewGuid().ToString();

            if (key != "0" && !string.IsNullOrWhiteSpace(key))
            {
                pnlBusesReference.Visibility = Visibility.Visible;
                LoadDepotData(key);
            }
        }

#if DEBUG
        private void FillDummyData()
        {
            var rnd = Random.Shared.Next(10, 99);
            txtState.Text = "Rajasthan";
            txtCity.Text = "Jaipur";
            txtDepot.Text = $"Central Depot {rnd}";
            txtDepotNumber.Text = $"DEP-RJ-{rnd}";
        }
#endif

        private async void LoadDepotData(string key)
        {
            try
            {
                var depot = await ApiClient.GetDepotByIdAsync(key);
                if (depot != null)
                {
                    txtState.Text = depot.State ?? string.Empty;
                    txtCity.Text = depot.City ?? string.Empty;
                    txtDepot.Text = depot.Depot ?? string.Empty;
                    txtDepotNumber.Text = depot.DepotNumber ?? string.Empty;
                    currentGuid = !string.IsNullOrWhiteSpace(depot.GUID) ? depot.GUID : Guid.NewGuid().ToString();

                    await LoadDepotBusesAsync(depot.GUID, depot.Depot, depot.DepotNumber);
                }
                else
                {
                    MessageBox.Show("Depot data nahi mila.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading depot: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private async Task LoadDepotBusesAsync(string? depotGuid, string? depotName, string? depotNumber)
        {
            try
            {
                txtBusesStatus.Text = "Loading buses for this depot...";
                txtBusesStatus.Visibility = Visibility.Visible;
                svBusesList.Visibility = Visibility.Collapsed;

                // NOTE: Apne ApiClient ke existing bus list method ke naam ke hisaab se adjust kar lena (e.g., GetBusesAsync / GetAllBusesAsync)
                var allBuses = await ApiClient.GetCachedBusesAsync();

                var matchedBuses = (allBuses ?? Enumerable.Empty<BusData>())
                    .Where(b =>
                        (!string.IsNullOrWhiteSpace(depotName) && string.Equals(b.Depot?.Trim(), depotName.Trim(), StringComparison.OrdinalIgnoreCase)))
                    .Select(b => new DepotBusRefItem
                    {
                        BusName = string.IsNullOrWhiteSpace(b.Depot) ? "Unnamed Bus" : b.Depot,
                        BusNumber = string.IsNullOrWhiteSpace(b.id) ? "N/A" : b.id,
                        RouteSummary = $"{b.From} → {b.To}" + (string.IsNullOrWhiteSpace(b.Departure) ? "" : $"  •  {b.Departure}")
                    })
                    .ToList();

                txtBusCount.Text = matchedBuses.Count == 1 ? "1 Bus" : $"{matchedBuses.Count} Buses";

                if (matchedBuses.Count == 0)
                {
                    txtBusesStatus.Text = "Is depot se abhi koi bus linked nahi hai.";
                    txtBusesStatus.Visibility = Visibility.Visible;
                    svBusesList.Visibility = Visibility.Collapsed;
                }
                else
                {
                    icDepotBuses.ItemsSource = matchedBuses;
                    txtBusesStatus.Visibility = Visibility.Collapsed;
                    svBusesList.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                txtBusCount.Text = "0 Buses";
                txtBusesStatus.Text = $"Buses load nahi ho sakin: {ex.Message}";
                txtBusesStatus.Visibility = Visibility.Visible;
                svBusesList.Visibility = Visibility.Collapsed;
            }
        }

        private void btnDiscard_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var state = txtState.Text?.Trim();
            var city = txtCity.Text?.Trim();
            var depotName = txtDepot.Text?.Trim();
            var depotNumber = txtDepotNumber.Text?.Trim();

            if (string.IsNullOrWhiteSpace(state) ||
                string.IsNullOrWhiteSpace(city) ||
                string.IsNullOrWhiteSpace(depotName) ||
                string.IsNullOrWhiteSpace(depotNumber))
            {
                MessageBox.Show("Please fill all fields before saving.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var depot = new DepotData
            {
                Id = isNew ? "0" : id,
                State = state,
                City = city,
                Depot = depotName,
                DepotNumber = depotNumber,
                GUID = string.IsNullOrWhiteSpace(currentGuid) ? Guid.NewGuid().ToString() : currentGuid
            };

            var (success, errorMsg) = await ApiClient.SaveDepotAsync(depot);
            if (success)
            {
                MessageBox.Show(isNew ? "New depot added successfully!" : "Depot updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            else
            {
                MessageBox.Show($"Server pe depot save nahi ho saka:\n{errorMsg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}