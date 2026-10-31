using Businfo.AddEdit;
using ClosedXML.Excel;
using Microsoft.Win32;
using Businfo.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Businfo.Manage
{
    public partial class Manage_Bus : Window
    {
        public ObservableCollection<BusData> DisplayedBuses { get; set; } = new ObservableCollection<BusData>();
        private List<BusData> masterBusList = new List<BusData>();
        private bool isInitialized = false;

        public Manage_Bus()
        {
            InitializeComponent();
            BusDataGrid.ItemsSource = DisplayedBuses;

            isInitialized = true;

            LoadVendorsDropdown();
            LoadBusData();
        }

        public async void LoadBusData()
        {
            try
            {
                var buses = await ApiClient.GetBusesAsync();
                masterBusList = buses ?? new List<BusData>();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Server se data load karne me error: {ex.Message}",
                    "Connection Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private async void LoadVendorsDropdown()
        {
            try
            {
                var meta = await ApiClient.GetMetadataAsync();
                var vendors = new List<string> { "All Vendors" };

                if (meta?.vendors != null)
                {
                    vendors.AddRange(meta.vendors);
                }

                cmbVendorFilter.ItemsSource = vendors;
                cmbVendorFilter.SelectedIndex = 0;
            }
            catch { }
        }

        private void ApplyFilters()
        {
            if (!isInitialized || SearchTextBox == null || cmbVendorFilter == null || masterBusList == null)
                return;

            var searchTxt = SearchTextBox.Text?.Trim().ToLower() ?? "";

            if (searchTxt == "search route, city, bus...")
                searchTxt = "";

            var selectedVendor = cmbVendorFilter.SelectedItem?.ToString();

            var filtered = masterBusList.Where(b =>
            {
                if (b == null) return false;

                bool matchVendor = string.IsNullOrEmpty(selectedVendor) ||
                                   selectedVendor == "All Vendors" ||
                                   string.Equals(b.Vendor, selectedVendor, StringComparison.OrdinalIgnoreCase);

                if (!matchVendor) return false;

                if (string.IsNullOrEmpty(searchTxt)) return true;

                return (!string.IsNullOrEmpty(b.Line) && b.Line.ToLower().Contains(searchTxt)) ||
                       (!string.IsNullOrEmpty(b.From) && b.From.ToLower().Contains(searchTxt)) ||
                       (!string.IsNullOrEmpty(b.To) && b.To.ToLower().Contains(searchTxt)) ||
                       (!string.IsNullOrEmpty(b.Route) && b.Route.ToLower().Contains(searchTxt)) ||
                       (!string.IsNullOrEmpty(b.Depot) && b.Depot.ToLower().Contains(searchTxt)) ||
                       (!string.IsNullOrEmpty(b.Vendor) && b.Vendor.ToLower().Contains(searchTxt));
            }).ToList();

            DisplayedBuses.Clear();

            foreach (var b in filtered)
            {
                DisplayedBuses.Add(b);
            }

            if (txtRecordCount != null)
            {
                txtRecordCount.Text = $"Showing {DisplayedBuses.Count} of {masterBusList.Count} routes";
            }
        }

        // Search Handlers
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "Search route, city, bus...")
            {
                SearchTextBox.Text = "";
                SearchTextBox.Foreground = Brushes.White;
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = "Search route, city, bus...";
                SearchTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8E92BC"));
            }
        }

        private void cmbVendorFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        // Add Bus
        private void btnAddBus_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditBus();
            addWindow.ShowDialog();
            LoadBusData();
        }

        // Edit Bus
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (BusDataGrid.SelectedItem is BusData a)
            {
                var s = new AddEditBus(a.id);
                s.ShowDialog();
                LoadBusData();
            }
        }

        // Clone Bus
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (BusDataGrid.SelectedItem is BusData s)
            {
                var cloned = s.Clone();
                var a = new AddEditBus(cloned);
                a.ShowDialog();
                LoadBusData();
            }
        }

        // Delete Bus
        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (BusDataGrid.SelectedItem is BusData s)
            {
                var result = MessageBox.Show(
                    $"Route '{s.Line}' ({s.From} -> {s.To}) delete karein?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    bool success = await ApiClient.DeleteBusAsync(s.id);

                    if (success)
                    {
                        LoadBusData();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Bus delete karne me error aaya.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                    }
                }
            }
        }
            
// Import Excel Button
private async void btnImportExcel_Click(object sender, RoutedEventArgs e)
{
    try
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Title = "Select RSRTC Bus Timetable Excel File",
            Filter = "Excel Workbook (*.xlsx)|*.xlsx",
            CheckFileExists = true
        };

        if (openFileDialog.ShowDialog() != true)
            return;

        // Load existing metadata
        var citiesData = await ApiClient.GetCitiesAsync();
        var metadata = await ApiClient.GetMetadataAsync();

        var cities = citiesData?
            .Where(c => !string.IsNullOrWhiteSpace(c.CityName))
            .Select(c => c.CityName.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();

        var busTypes = metadata?.busTypes ?? new List<string>();
        var depots = metadata?.depots ?? new List<string>();

        if (cities.Count == 0 || busTypes.Count == 0 || depots.Count == 0)
        {
            MessageBox.Show(
                "Existing Cities, Bus Types ya Depots ka data load nahi ho paya. Internet/API connection check karein.",
                "Metadata Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            return;
        }

        var importedBuses = new List<BusData>();
        var errors = new List<string>();

        using (var workbook = new XLWorkbook(openFileDialog.FileName))
        {
            var worksheet = workbook.Worksheet("Bus Timetable");
            int lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

            BusData currentBus = null;

            for (int row = 2; row <= lastRow; row++)
            {
                string GetValue(int column)
                {
                    return worksheet.Cell(row, column).GetFormattedString().Trim();
                }

                string line = GetValue(1);
                string busType = GetValue(2);
                string depot = GetValue(3);
                string from = GetValue(4);
                string departure = GetValue(5);
                string to = GetValue(6);
                string arrival = GetValue(7);
                string route = GetValue(8);

                string station = GetValue(9);
                string stopArrival = GetValue(10);
                string stopDeparture = GetValue(11);
                string distance = GetValue(12);
                string day = GetValue(13);

                bool rowIsEmpty = Enumerable.Range(1, 13)
                    .All(c => string.IsNullOrWhiteSpace(
                        worksheet.Cell(row, c).GetFormattedString()));

                if (rowIsEmpty)
                    continue;

                // A non-empty Bus Line starts a new bus
                if (!string.IsNullOrWhiteSpace(line))
                {
                    if (currentBus != null)
                        importedBuses.Add(currentBus);

                    string matchedFrom = cities.FirstOrDefault(
                        c => string.Equals(c, from, StringComparison.OrdinalIgnoreCase));

                    string matchedTo = cities.FirstOrDefault(
                        c => string.Equals(c, to, StringComparison.OrdinalIgnoreCase));

                    string matchedBusType = busTypes.FirstOrDefault(
                        b => string.Equals(b?.Trim(), busType, StringComparison.OrdinalIgnoreCase));

                    string matchedDepot = depots.FirstOrDefault(
                        d => string.Equals(d?.Trim(), depot, StringComparison.OrdinalIgnoreCase));

                    if (string.IsNullOrWhiteSpace(busType) ||
                        matchedBusType == null)
                        errors.Add($"Row {row}: Invalid or missing Bus Type: {busType}");

                    if (string.IsNullOrWhiteSpace(depot) ||
                        matchedDepot == null)
                        errors.Add($"Row {row}: Invalid or missing Depot: {depot}");

                    if (string.IsNullOrWhiteSpace(from) || matchedFrom == null)
                        errors.Add($"Row {row}: From City does not exist: {from}");

                    if (string.IsNullOrWhiteSpace(to) || matchedTo == null)
                        errors.Add($"Row {row}: To City does not exist: {to}");

                    if (string.IsNullOrWhiteSpace(departure))
                        errors.Add($"Row {row}: Departure is missing.");

                    if (string.IsNullOrWhiteSpace(arrival))
                        errors.Add($"Row {row}: Arrival is missing.");

                    if (string.IsNullOrWhiteSpace(route))
                        errors.Add($"Row {row}: Route is missing.");

                    currentBus = new BusData
                    {
                        id = "0",
                        Line = line,
                        BusType = matchedBusType ?? busType,
                        Depot = matchedDepot ?? depot,
                        From = matchedFrom ?? from,
                        To = matchedTo ?? to,
                        Time = $"{departure},{arrival}",
                        Route = route,
                        Vendor = "RSRTC",
                        GUID = Guid.NewGuid().ToString(),
                        Stops = new List<StopData>()
                    };
                }
                else if (currentBus == null)
                {
                    errors.Add($"Row {row}: Bus Line missing. Har nayi bus ke first row mein Bus Line zaroor likhein.");
                    continue;
                }

                // Add stoppage when any stoppage field is filled
                bool hasStopData =
                    !string.IsNullOrWhiteSpace(station) ||
                    !string.IsNullOrWhiteSpace(stopArrival) ||
                    !string.IsNullOrWhiteSpace(stopDeparture) ||
                    !string.IsNullOrWhiteSpace(distance) ||
                    !string.IsNullOrWhiteSpace(day);

                if (hasStopData)
                {
                    string matchedStation = cities.FirstOrDefault(
                        c => string.Equals(c, station, StringComparison.OrdinalIgnoreCase));

                    if (matchedStation == null)
                    {
                        errors.Add($"Row {row}: Stoppage City does not exist: {station}");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(stopArrival) ||
                        string.IsNullOrWhiteSpace(stopDeparture) ||
                        string.IsNullOrWhiteSpace(distance) ||
                        string.IsNullOrWhiteSpace(day))
                    {
                        errors.Add($"Row {row}: Stoppage ke Arrival, Departure, Distance aur Day sab bharna zaroori hai.");
                        continue;
                    }

                    currentBus.Stops.Add(new StopData
                    {
                        Station = matchedStation,
                        Arrival = stopArrival,
                        Departure = stopDeparture,
                        Distance = distance,
                        Day = day,
                        GUID = Guid.NewGuid().ToString()
                    });
                }
            }

            if (currentBus != null)
                importedBuses.Add(currentBus);
        }

        // Do not save anything if Excel has validation errors
        if (errors.Count > 0)
        {
            string errorReport = string.Join(
                Environment.NewLine,
                errors.Take(25)
            );

            if (errors.Count > 25)
                errorReport += Environment.NewLine +
                    $"...aur {errors.Count - 25} errors.";

            MessageBox.Show(
                "Excel mein kuch errors mile hain. Koi bus save nahi hui.\n\n" +
                errorReport,
                "Excel Validation Failed",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );

            return;
        }

        if (importedBuses.Count == 0)
        {
            MessageBox.Show(
                "Excel file mein koi bus record nahi mila.",
                "No Records",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }

        var confirm = MessageBox.Show(
            $"Excel mein {importedBuses.Count} buses mili hain.\n\n" +
            "Vendor: RSRTC\n" +
            "Existing Cities, Bus Types aur Depots verify ho chuke hain.\n\n" +
            "Kya aap in buses ko server par import karna chahte hain?",
            "Confirm Excel Import",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );

        if (confirm != MessageBoxResult.Yes)
            return;

        int successCount = 0;
        var failedRecords = new List<string>();

        foreach (var bus in importedBuses)
        {
            var result = await ApiClient.SaveBusAsync(bus, true);

            if (result.Success)
            {
                successCount++;
            }
            else
            {
                failedRecords.Add(
                    $"{bus.Line} ({bus.From} → {bus.To}): {result.Message}"
                );
            }
        }

        LoadBusData();

        string finalMessage =
            $"Import Complete!\n\n" +
            $"Successfully Saved: {successCount}\n" +
            $"Failed: {failedRecords.Count}";

        if (failedRecords.Count > 0)
        {
            finalMessage += "\n\nFailed Records:\n" +
                string.Join("\n", failedRecords.Take(10));
        }

        MessageBox.Show(
            finalMessage,
            "RSRTC LifeLine Import Result",
            MessageBoxButton.OK,
            failedRecords.Count == 0
                ? MessageBoxImage.Information
                : MessageBoxImage.Warning
        );
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Excel Import Error:\n" + ex.Message,
            "Import Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error
        );
    }
}


        // Download Excel Template
        private void btnDownloadTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Title = "Download RSRTC Excel Template",
                    FileName = "RSRTC_Bus_Timetable_Template.xlsx",
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Bus Timetable");

                        string[] headers =
                        {
                            "Bus Line",
                            "Bus Type",
                            "Depot",
                            "From",
                            "Departure",
                            "To",
                            "Arrival",
                            "Route",
                            "Stoppage",
                            "Stop Arrival",
                            "Stop Departure",
                            "Distance KM",
                            "Day"
                        };

                        for (int i = 0; i < headers.Length; i++)
                        {
                            worksheet.Cell(1, i + 1).Value = headers[i];
                        }

                        var header = worksheet.Range(1, 1, 1, headers.Length);
                        header.Style.Font.Bold = true;
                        header.Style.Font.FontColor = XLColor.White;
                        header.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        worksheet.SheetView.FreezeRows(1);
                        worksheet.Columns().AdjustToContents();

                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    MessageBox.Show(
                        "Excel Template successfully saved!",
                        "RSRTC LifeLine",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Template banane me error: " + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
