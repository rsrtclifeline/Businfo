#nullable enable
using Businfo.Helper;
using Businfo.Manage;
using System;
using System.Windows;

namespace Businfo.AddEdit
{
    public partial class AddEditDepot : Window
    {
        private string id = "0";
        private readonly bool isNew = true;
        private string currentGuid = string.Empty;

        // 1. Add New Constructor
        public AddEditDepot()
        {
            InitializeComponent();
            isNew = true;
            id = "0";
            currentGuid = Guid.NewGuid().ToString();

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
        }

        // 3. Fallback Key/Id Constructor (Sirf tab chalega agar direct model na ho)
        public AddEditDepot(string key)
        {
            InitializeComponent();
            this.id = key;
            isNew = false;
            currentGuid = Guid.NewGuid().ToString();

            // Agar Id '0' ya empty hai toh bekar HTTP call mat karo
            if (key != "0" && !string.IsNullOrWhiteSpace(key))
            {
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