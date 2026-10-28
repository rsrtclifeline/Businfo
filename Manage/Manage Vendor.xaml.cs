using Businfo.AddEdit;
using Businfo.Helper;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace Businfo.Manage
{
    public partial class Manage_Vendor : Window
    {
        public ObservableCollection<VendorModel> VendorList { get; set; } = new ObservableCollection<VendorModel>();

        public Manage_Vendor()
        {
            InitializeComponent();
            VendorDataGrid.ItemsSource = VendorList;
            LoadVendors();
        }

        public async void LoadVendors()
        {
            try
            {
                var vendors = await ApiClient.GetVendorsAsync();
                VendorList.Clear();
                if (vendors != null)
                {
                    foreach (var v in vendors)
                    {
                        VendorList.Add(v);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Vendors load karne me error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Edit
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (VendorDataGrid.SelectedItem is VendorModel selected)
            {
                var editWindow = new AddEditVendor(selected.DocId);
                editWindow.ShowDialog();
                LoadVendors(); // Window band hone par fresh data auto-refresh
            }
        }

        // Delete
        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (VendorDataGrid.SelectedItem is VendorModel selected)
            {
                var result = MessageBox.Show($"Are you sure you want to delete {selected.VendorName}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    bool success = await ApiClient.DeleteVendorAsync(selected.DocId);
                    if (success)
                    {
                        LoadVendors();
                    }
                    else
                    {
                        MessageBox.Show("Delete karne me error aaya.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // Add New Vendor
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditVendor();
            addWindow.ShowDialog();
            LoadVendors(); // Naya add hone par list refresh
        }
    }
}