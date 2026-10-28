using Businfo.AddEdit;
using Businfo.Helper;
using Businfo.Login;
using Businfo.Manage;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Businfo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer refreshTimer;

        public MainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed;

            // Pehli baar load karo
            LoadDashboardCounts();

            // Har 5 second me auto update ke liye simple timer
            refreshTimer = new DispatcherTimer();
            refreshTimer.Interval = TimeSpan.FromSeconds(5);
            refreshTimer.Tick += (s, e) => LoadDashboardCounts();
            refreshTimer.Start();
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            refreshTimer?.Stop();
            Application.Current.Shutdown();
        }

        private async void LoadDashboardCounts()
        {
            var counts = await ApiClient.GetDashboardCountsAsync();
            if (counts == null) return;

            UpdateCountText("CityInfo_Count", counts.CityInfo);
            UpdateCountText("VendorInfo_Count", counts.VendorInfo);
            UpdateCountText("BusInfo_Count", counts.BusInfo);
            UpdateCountText("Depots_Count", counts.Depots);
        }

        private void UpdateCountText(string textBoxName, int count)
        {
            if (this.FindName(textBoxName) is TextBlock tb)
            {
                tb.Text = count.ToString();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowHelper.ShowSingleInstance<Manage_Bus>();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            WindowHelper.ShowSingleInstance<Manage_City>();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            WindowHelper.ShowSingleInstance<AddEditCity>();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            WindowHelper.ShowSingleInstance<AddEditBus>();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            WindowHelper.ShowSingleInstance<Manage_Vendor>();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            WindowHelper.ShowSingleInstance<AddEditVendor>();
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            WindowHelper.ShowSingleInstance<Manage_Depot>();
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            WindowHelper.ShowSingleInstance<Manage_Bustype>();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            WindowHelper.ShowSingleInstance<LoginWindow>();
        }
    }
}