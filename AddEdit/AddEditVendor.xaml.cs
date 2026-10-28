using Businfo.Helper;
using Businfo.Manage;
using System;
using System.Windows;

namespace Businfo.AddEdit
{
    public partial class AddEditVendor : Window
    {
        private bool isNew = true;
        private string docId = "0";

        public AddEditVendor()
        {
            InitializeComponent();
            isNew = true;
            docId = "0";

#if DEBUG
            FillDummyData();
#endif
        }

        public AddEditVendor(string id)
        {
            InitializeComponent();
            docId = id;
            isNew = false;
            LoadVendorById(id);
        }

#if DEBUG
        private void FillDummyData()
        {
            var rnd = new Random().Next(100, 999);
            txtVendorName.Text = $"Sharma Travels {rnd}";
            txtNoOfVehicle.Text = "25";
            txtContact1.Text = "9876543210";
            txtContact2.Text = "9123456780";
            txtEmail.Text = $"contact{rnd}@sharmatravels.com";
            txtWebsite.Text = "https://sharmatravels.com";
            txtAddress.Text = "Sindhi Camp Bus Stand, Jaipur, Rajasthan";
            txtAbout.Text = "Authorized RSRTC luxury bus franchise partner since 2018.";
        }
#endif

        private async void LoadVendorById(string id)
        {
            try
            {
                var vendor = await ApiClient.GetVendorByIdAsync(id);
                if (vendor != null)
                {
                    txtVendorName.Text = vendor.VendorName ?? "";
                    txtNoOfVehicle.Text = vendor.NoOfVehicle ?? "";
                    txtContact1.Text = vendor.Contact1 ?? "";
                    txtContact2.Text = vendor.Contact2 ?? "";
                    txtEmail.Text = vendor.Email ?? "";
                    txtWebsite.Text = vendor.Website ?? "";
                    txtAddress.Text = vendor.Address ?? "";
                    txtAbout.Text = vendor.About ?? "";
                }
                else
                {
                    MessageBox.Show("Vendor data nahi mila.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vendor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e) // Save Button
        {
            var vendorName = txtVendorName.Text?.Trim();

            if (string.IsNullOrWhiteSpace(vendorName))
            {
                MessageBox.Show("Vendor Name bharna anivarya hai.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var model = new VendorModel
            {
                DocId = isNew ? "0" : docId,
                VendorName = vendorName,
                NoOfVehicle = txtNoOfVehicle.Text?.Trim() ?? "",
                Contact1 = txtContact1.Text?.Trim() ?? "",
                Contact2 = txtContact2.Text?.Trim() ?? "",
                Email = txtEmail.Text?.Trim() ?? "",
                Website = txtWebsite.Text?.Trim() ?? "",
                Address = txtAddress.Text?.Trim() ?? "",
                About = txtAbout.Text?.Trim() ?? ""
            };

            var (success, errorMsg) = await ApiClient.SaveVendorAsync(model);
            if (success)
            {
                MessageBox.Show(isNew ? "New vendor added successfully!" : "Vendor updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show($"Server pe vendor save/update nahi ho saka:\n{errorMsg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}