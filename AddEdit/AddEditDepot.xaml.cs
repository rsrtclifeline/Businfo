using Businfo.Helper;
using Businfo.Manage;
using System;
using System.Windows;

namespace Businfo.AddEdit
{
    public partial class AddEditDepot : Window
    {
        private string id = "0";
        private bool isNew = true;
        private string currentGuid = string.Empty;

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

        public AddEditDepot(string id)
        {
            InitializeComponent();
            this.id = id;
            isNew = false;
            LoadDepotData();
        }

#if DEBUG
        private void FillDummyData()
        {
            var rnd = new Random().Next(10, 99);
            txtState.Text = "Rajasthan";
            txtCity.Text = "Jaipur";
            txtDepot.Text = $"Central Depot {rnd}";
            txtDepotNumber.Text = $"DEP-RJ-{rnd}";
        }
#endif

        private async void LoadDepotData()
        {
            try
            {
                var depot = await ApiClient.GetDepotByIdAsync(id);
                if (depot != null)
                {
                    txtState.Text = depot.State ?? "";
                    txtCity.Text = depot.City ?? "";
                    txtDepot.Text = depot.Depot ?? "";
                    txtDepotNumber.Text = depot.DepotNumber ?? "";
                    currentGuid = string.IsNullOrWhiteSpace(depot.GUID) ? Guid.NewGuid().ToString() : depot.GUID;
                }
                else
                {
                    MessageBox.Show("Depot data nahi mila.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading depot: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void btnDiscard_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
                this.Close();
            }
            else
            {
                MessageBox.Show($"Server pe depot save nahi ho saka:\n{errorMsg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}