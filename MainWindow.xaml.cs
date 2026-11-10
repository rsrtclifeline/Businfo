using Businfo.AddEdit;
using Businfo.Helper;
using Businfo.Login;
using Businfo.Manage;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Businfo
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer? fallbackTimer;

        public MainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed;

            txtOperatorName.Text = string.IsNullOrWhiteSpace(ApiClient.CurrentUsername) ? "Operator" : ApiClient.CurrentUsername;

            // Initial API Load
            LoadDashboardCounts();

            // Realtime Socket Event
            SetupRealtimeSocket();

            // Fallback Timer
            fallbackTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(60)
            };
            fallbackTimer.Tick += (s, e) => LoadDashboardCounts();
            fallbackTimer.Start();
        }

        private void SetupRealtimeSocket()
        {
            if (ApiClient.Socket == null) return;

            ApiClient.Socket.On("dashboard_counts_updated", async context =>
            {
                try
                {
                    if (context == null) return;

                    // Window close/shutdown ho rahi ho toh aage mat badho
                    if (Dispatcher == null || Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished)
                        return;

                    // index 0 mandatory hai yaha
                    DashboardCounts counts = context.GetValue<DashboardCounts>(0);

                    if (counts == null) return;

                    if (!Dispatcher.HasShutdownStarted)
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            ApplyCountsToUI(counts);
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($">>> Realtime counts skipped: {ex.Message}");
                }
            });
        }

        private async void LoadDashboardCounts()
        {
            try
            {
                var counts = await ApiClient.GetDashboardCountsAsync();
                if (counts == null) return;

                if (Dispatcher != null && !Dispatcher.HasShutdownStarted)
                {
                    ApplyCountsToUI(counts);
                }
            }
            catch { }
        }

        private void ApplyCountsToUI(DashboardCounts counts)
        {
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

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show("Kya aap logout karna chahte hain?", "Confirm Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.Yes)
            {
                fallbackTimer?.Stop();
                ApiClient.Logout();

                var login = new LoginWindow();
                login.Show();
                this.Close();
            }
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            fallbackTimer?.Stop();

            // SocketIO.cs ko bina modify kiye listener ko unhook/mute karna
            try
            {
                ApiClient.Socket?.On("dashboard_counts_updated", _ => Task.CompletedTask);
            }
            catch { }

            Application.Current?.Shutdown();
        }

        // Navigation Clicks
        private void Button_Click(object sender, RoutedEventArgs e) => WindowHelper.ShowSingleInstance<Manage_Bus>();
        private void Button_Click_1(object sender, RoutedEventArgs e) => WindowHelper.ShowSingleInstance<Manage_City>();
        private void Button_Click_2(object sender, RoutedEventArgs e) => WindowHelper.ShowSingleInstance<AddEditVendor>();
        private void Button_Click_3(object sender, RoutedEventArgs e) => WindowHelper.ShowSingleInstance<Manage_Vendor>();
        private void Button_Click_4(object sender, RoutedEventArgs e) => WindowHelper.ShowSingleInstance<AddEditCity>();
        private void Button_Click_5(object sender, RoutedEventArgs e) => WindowHelper.ShowSingleInstance<AddEditBus>();
        private void Button_Click_7(object sender, RoutedEventArgs e) => WindowHelper.ShowSingleInstance<Manage_Bustype>();
        private void Button_Click_8(object sender, RoutedEventArgs e) => WindowHelper.ShowSingleInstance<Manage_Depot>();
        private void MenuItem_Click(object sender, RoutedEventArgs e) => WindowHelper.ShowSingleInstance<LoginWindow>();
    }
}