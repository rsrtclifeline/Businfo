#nullable enable
using Businfo.Helper;
using Businfo.Manage;
using System;
using System.Windows;

namespace Businfo.AddEdit
{
    public partial class AddEditBusType : Window
    {
        private string id = "0";
        private readonly bool isNew = true;
        private string currentGuid = string.Empty;

        // 1. Add New Constructor
        public AddEditBusType()
        {
            InitializeComponent();
            isNew = true;
            id = "0";
            currentGuid = Guid.NewGuid().ToString();

#if DEBUG
            FillDummyData();
#endif
        }

        // 2. Direct Model Injection Constructor (0ms launch, zero network overhead)
        public AddEditBusType(BusTypeModel model)
        {
            InitializeComponent();
            isNew = false;
            id = model.id ?? "0";
            currentGuid = !string.IsNullOrWhiteSpace(model.GUID) ? model.GUID : Guid.NewGuid().ToString();

            txtBusType.Text = model.BusType ?? string.Empty;
        }

        // 3. Fallback ID Constructor (Only when instance is not already in memory)
        public AddEditBusType(string id)
        {
            InitializeComponent();
            this.id = id;
            isNew = false;
            currentGuid = Guid.NewGuid().ToString();

            LoadBusTypeData();
        }

#if DEBUG
        private void FillDummyData()
        {
            var types = new[] { "Express", "Super Express", "AC Sleeper 2+1", "Ordinary non-stop", "Volvo Multi-Axle" };
            txtBusType.Text = types[Random.Shared.Next(types.Length)];
        }
#endif

        private async void LoadBusTypeData()
        {
            try
            {
                var data = await ApiClient.GetBusTypeByIdAsync(id);
                if (data != null)
                {
                    txtBusType.Text = data.BusType ?? string.Empty;
                    currentGuid = string.IsNullOrWhiteSpace(data.GUID) ? Guid.NewGuid().ToString() : data.GUID;
                }
                else
                {
                    MessageBox.Show("Bus Type data nahi mila.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bus type: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var typeName = txtBusType.Text?.Trim();

            if (string.IsNullOrWhiteSpace(typeName))
            {
                MessageBox.Show("Bus Type bharna anivarya hai.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var model = new BusTypeModel
            {
                id = isNew ? "0" : id,
                BusType = typeName,
                GUID = string.IsNullOrWhiteSpace(currentGuid) ? Guid.NewGuid().ToString() : currentGuid
            };

            var (success, errorMsg) = await ApiClient.SaveBusTypeAsync(model);
            if (success)
            {
                MessageBox.Show(isNew ? "New bus type added successfully!" : "Bus type updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            else
            {
                MessageBox.Show($"Server pe bus type save nahi ho saka:\n{errorMsg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDiscard_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}