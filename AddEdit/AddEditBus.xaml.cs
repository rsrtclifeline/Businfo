#nullable enable
using Businfo.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Businfo.AddEdit
{
    public partial class AddEditBus : Window
    {
        public ObservableCollection<string> Vendors { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> CitiesFROM { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> CitiesTO { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> BusType { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Depot { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<StopData> Stops { get; set; } = new ObservableCollection<StopData>();

        private BusData? currentBus;
        private bool isNew = true;
        private bool reUse = false;

        private DataGridRow? draggedRow;
        private Point startPoint;

        // 1. Add New Bus
        public AddEditBus()
        {
            InitializeComponent();
            SetupBindings();
            _ = InitializeNewBusAsync();
        }

        private async Task InitializeNewBusAsync()
        {
            await LoadMetadataAsync();
#if DEBUG
            FillDummyData();
#endif
        }

        // 2. Edit by ID
        public AddEditBus(string id)
        {
            InitializeComponent();
            SetupBindings();
            isNew = false;
            _ = InitializeEditBusByIdAsync(id);
        }

        private async Task InitializeEditBusByIdAsync(string id)
        {
            await LoadMetadataAsync();
            await LoadBusDataAsync(id);
        }

        // 3. Edit / Re-use Direct Bus Instance (AddEditCity se yahi khulta hai)
        public AddEditBus(BusData bus)
        {
            InitializeComponent();
            SetupBindings();

            // Existing bus ko modify kar rahe hain
            currentBus = bus;
            isNew = false;
            reUse = true;

            _ = InitializeDirectBusAsync(bus);
        }

        private async Task InitializeDirectBusAsync(BusData bus)
        {
            // Dropdowns pehle load honge, phir text/selection bind hoga!
            await LoadMetadataAsync();
            ApplyBusDataToForm(bus);
        }

        private void SetupBindings()
        {
            StopsDataGrid.ItemsSource = Stops;
            combo_From.ItemsSource = CitiesFROM;
            combo_To.ItemsSource = CitiesTO;
            combo_Vendor.ItemsSource = Vendors;
            combo_BusType.ItemsSource = BusType;
            combo_Depot.ItemsSource = Depot;
        }

        private void ApplyBusDataToForm(BusData bus)
        {
            combo_line.Text = bus.Line ?? string.Empty;
            combo_From.Text = bus.From ?? string.Empty;
            combo_To.Text = bus.To ?? string.Empty;
            route.Text = bus.Route ?? string.Empty;
            combo_Vendor.Text = bus.Vendor ?? string.Empty;
            combo_BusType.Text = bus.BusType ?? string.Empty;
            combo_Depot.Text = bus.Depot ?? string.Empty;

            Stops.Clear();
            if (bus.Stops != null)
            {
                foreach (var stop in bus.Stops)
                {
                    Stops.Add(stop.Clone());
                }
            }
        }

#if DEBUG
        private void FillDummyData()
        {
            var rnd = new Random().Next(10, 99);
            combo_line.Text = $"Express Line {rnd}";
            combo_From.Text = "Jaipur";
            combo_To.Text = "Delhi";
            route.Text = "Jaipur - Kotputli - Behror - Gurgaon - Delhi";
            combo_Vendor.Text = "Sharma Travels";
            combo_BusType.Text = "Express";
            combo_Depot.Text = "Central Depot";

            Stops.Clear();
            Stops.Add(new StopData { Station = "Jaipur", Arrival = "06:00 AM", Departure = "06:15 AM", Distance = "0 km", Day = "Day 1", GUID = Guid.NewGuid().ToString() });
            Stops.Add(new StopData { Station = "Kotputli", Arrival = "08:00 AM", Departure = "08:10 AM", Distance = "105 km", Day = "Day 1", GUID = Guid.NewGuid().ToString() });
            Stops.Add(new StopData { Station = "Delhi", Arrival = "11:30 AM", Departure = "11:45 AM", Distance = "270 km", Day = "Day 1", GUID = Guid.NewGuid().ToString() });
        }
#endif

        private async Task LoadMetadataAsync()
        {
            try
            {
                var meta = await ApiClient.GetMetadataAsync();
                if (meta != null)
                {
                    CitiesFROM.Clear();
                    CitiesTO.Clear();
                    Vendors.Clear();
                    BusType.Clear();
                    Depot.Clear();

                    if (meta.cities != null)
                    {
                        foreach (var c in meta.cities) { CitiesFROM.Add(c); CitiesTO.Add(c); }
                    }
                    if (meta.vendors != null)
                    {
                        foreach (var v in meta.vendors) Vendors.Add(v);
                    }
                    if (meta.busTypes != null)
                    {
                        foreach (var bt in meta.busTypes) BusType.Add(bt);
                    }
                    if (meta.depots != null)
                    {
                        foreach (var d in meta.depots) Depot.Add(d);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Metadata load karne me error: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task LoadBusDataAsync(string id)
        {
            try
            {
                currentBus = await ApiClient.GetBusByIdAsync(id);
                if (currentBus == null)
                {
                    MessageBox.Show("Bus data nahi mila.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                ApplyBusDataToForm(currentBus);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching bus: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void Button_Click_AddStop(object sender, RoutedEventArgs e)
        {
            var addStopWindow = new AddEditStop();
            addStopWindow.Result = (idx, d) => Stops.Add(d);
            addStopWindow.ShowDialog();
        }

        private void StopUpdateEvent(int index, StopData stopData)
        {
            if (index >= 0 && index < Stops.Count)
                Stops[index] = stopData;
            else
                Stops.Add(stopData);
        }

        private void Button_Click_SaveBus(object sender, RoutedEventArgs e)
        {
            SaveBusAsync();
        }

        private async void SaveBusAsync()
        {
            if (string.IsNullOrWhiteSpace(combo_line.Text) ||
                string.IsNullOrWhiteSpace(combo_From.Text) ||
                string.IsNullOrWhiteSpace(combo_To.Text) ||
                string.IsNullOrWhiteSpace(route.Text) ||
                string.IsNullOrWhiteSpace(combo_Vendor.Text) ||
                string.IsNullOrWhiteSpace(combo_BusType.Text) ||
                string.IsNullOrWhiteSpace(combo_Depot.Text))
            {
                MessageBox.Show("Sabhi fields bharna zaroori hai.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var busPayload = new BusData
            {
                id = isNew ? "0" : currentBus?.id,
                Line = combo_line.Text.Trim(),
                From = combo_From.Text.Trim(),
                To = combo_To.Text.Trim(),
                Route = route.Text.Trim(),
                Vendor = combo_Vendor.Text.Trim(),
                BusType = combo_BusType.Text.Trim(),
                Depot = combo_Depot.Text.Trim(),
                GUID = isNew ? Guid.NewGuid().ToString() : (currentBus?.GUID ?? Guid.NewGuid().ToString()),
                Stops = Stops.ToList()
            };

            var (success, errorMsg) = await ApiClient.SaveBusAsync(busPayload, isNew);
            if (success)
            {
                MessageBox.Show(isNew ? "Bus successfully add ho gayi!" : "Bus successfully update ho gayi!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                if (isNew && !reUse)
                {
                    Button_Click_1(sender: this, e: new RoutedEventArgs()); // Clear Form
                }
                else
                {
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show($"Server par data save nahi ho paya:\n{errorMsg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Reverse Bus Route & Stops
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var reversedStops = Stops.Reverse().ToList();
            Stops.Clear();

            foreach (var stop in reversedStops)
            {
                var tempArrival = stop.Arrival;
                stop.Arrival = stop.Departure;
                stop.Departure = tempArrival;
                Stops.Add(stop);
            }

            var tempFrom = combo_From.Text;
            combo_From.Text = combo_To.Text;
            combo_To.Text = tempFrom;
        }

        // Reset Form
        private void Button_Click_1(object? sender, RoutedEventArgs? e)
        {
            combo_line.Text = string.Empty;
            combo_From.Text = string.Empty;
            combo_To.Text = string.Empty;
            route.Text = string.Empty;
            combo_Vendor.Text = string.Empty;
            combo_BusType.Text = string.Empty;
            combo_Depot.Text = string.Empty;
            Stops.Clear();
            currentBus = null;
            isNew = true;
        }

        // Edit Selected Stop Dialog
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (StopsDataGrid.SelectedItem is StopData s)
            {
                var i = Stops.IndexOf(s);
                var marao = new AddEditStop(s, i);
                marao.Result = (idx, d) => StopUpdateEvent(idx, d);
                marao.ShowDialog();
            }
        }

        // Delete Selected Stop
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (StopsDataGrid.SelectedItem is StopData s)
            {
                Stops.Remove(s);
            }
        }

        // Insert Stop Below
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var i = StopsDataGrid.SelectedIndex;
            var j = i + 1;

            var newStop = new StopData
            {
                Station = "",
                Arrival = "",
                Departure = "",
                Distance = "",
                Day = "",
                GUID = Guid.NewGuid().ToString()
            };

            if (i >= 0 && j <= Stops.Count)
                Stops.Insert(j, newStop);
            else
                Stops.Add(newStop);
        }

        // Drag and Drop
        private void StopsDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
            var row = FindAncestor<DataGridRow>((DependencyObject)e.OriginalSource);
            if (row != null)
                draggedRow = row;
        }

        public static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current != null && !(current is T))
                current = VisualTreeHelper.GetParent(current);
            return current as T;
        }

        private void StopsDataGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && draggedRow != null)
            {
                Point currentPos = e.GetPosition(null);
                Vector diff = startPoint - currentPos;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    DragDrop.DoDragDrop(draggedRow, draggedRow.Item, DragDropEffects.Move);
                }
            }
        }

        private void StopsDataGrid_Drop(object sender, DragEventArgs e)
        {
            var droppedData = e.Data.GetData(typeof(StopData)) as StopData;
            var target = FindAncestor<DataGridRow>((DependencyObject)e.OriginalSource)?.Item as StopData;

            if (droppedData == null || target == null || droppedData == target)
                return;

            int removedIdx = Stops.IndexOf(droppedData);
            int targetIdx = Stops.IndexOf(target);

            if (removedIdx == -1 || targetIdx == -1)
                return;

            if (removedIdx < targetIdx)
            {
                Stops.Insert(targetIdx + 1, droppedData);
                Stops.RemoveAt(removedIdx);
            }
            else
            {
                Stops.RemoveAt(removedIdx);
                Stops.Insert(targetIdx, droppedData);
            }
        }
    }
}