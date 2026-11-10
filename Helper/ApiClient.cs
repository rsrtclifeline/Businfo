using Businfo.Manage;
using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Businfo.Helper
{
    public static class ApiClient
    {
        public static readonly string BaseUrl = "https://rsrtc-lifeline.yhgindian.com";
        private static readonly HttpClient http = new HttpClient();

        // ================= SOCKET.IO CLIENT =================
        public static SocketIOClient.SocketIO Socket { get; private set; }

        public static string AuthToken { get; private set; } = string.Empty;
        public static string CurrentUsername { get; set; } = string.Empty;
        public static string CurrentDisplayName { get; set; } = string.Empty;

        public static List<BusData> BusCache { get; private set; } = new List<BusData>();
        public static bool IsBusCacheLoaded { get; private set; } = false;

        public static MetadataResponse MetadataCache { get; private set; } = new MetadataResponse();
        public static bool IsMetadataLoaded { get; private set; } = false;
        static ApiClient()
        {
            http.BaseAddress = new Uri(BaseUrl);
            http.DefaultRequestHeaders.Add("x-admin-key", "RSRTC_ADMIN_SUPER_SECRET_2026");

            InitializeSocket();
        }

        private static void InitializeSocket()
        {
            try
            {
                Socket = new SocketIOClient.SocketIO(new Uri(BaseUrl), new SocketIOOptions
                {
                    Reconnection = true,
                    ReconnectionAttempts = int.MaxValue,
                    ReconnectionDelayMax = 1000,
                    ExtraHeaders = new Dictionary<string, string>
                    {
                        { "x-admin-key", "RSRTC_ADMIN_SUPER_SECRET_2026" }
                    }
                });

                Socket.OnConnected += (sender, e) =>
                {
                    System.Diagnostics.Debug.WriteLine(">>> Socket.IO connected successfully to " + BaseUrl);
                };

                Socket.OnDisconnected += (sender, e) =>
                {
                    System.Diagnostics.Debug.WriteLine(">>> Socket.IO disconnected: " + e);
                };

                _ = Socket.ConnectAsync();

                SetupBusCacheSocketSync();
                SetupMetadataSocketSync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(">>> Socket initialization failed: " + ex.Message);
            }
        }

        public static void SetAuthToken(string token)
        {
            AuthToken = token;
            http.DefaultRequestHeaders.Remove("Authorization");
            if (!string.IsNullOrWhiteSpace(token))
            {
                http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            }

            // Socket reconnect with updated Auth token header
            try
            {
                if (Socket != null)
                {
                    // Nayi dictionary clone banao existing headers ke saath
                    var headers = new Dictionary<string, string>();

                    if (Socket.Options.ExtraHeaders != null)
                    {
                        foreach (var kvp in Socket.Options.ExtraHeaders)
                        {
                            if (!kvp.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                            {
                                headers[kvp.Key] = kvp.Value;
                            }
                        }
                    }

                    // Naya token add karo agar present hai
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        headers["Authorization"] = $"Bearer {token}";
                    }

                    // Fresh headers dictionary assign karo
                    Socket.Options.ExtraHeaders = headers;

                    if (!Socket.Connected)
                    {
                        _ = Socket.ConnectAsync();
                    }
                }
            }
            catch { }
        }

        public static async Task<DashboardCounts> GetDashboardCountsAsync()
        {
            try
            {
                return await http.GetFromJsonAsync<DashboardCounts>("/api/admin/dashboard-counts");
            }
            catch
            {
                return new DashboardCounts();
            }
        }

        // ================= VENDOR METHODS =================

        public static async Task<List<VendorModel>> GetVendorsAsync()
        {
            try
            {
                return await http.GetFromJsonAsync<List<VendorModel>>("/api/admin/vendors") ?? new List<VendorModel>();
            }
            catch
            {
                return new List<VendorModel>();
            }
        }

        public static async Task<VendorModel> GetVendorByIdAsync(string id)
        {
            try
            {
                return await http.GetFromJsonAsync<VendorModel>($"/api/admin/vendors/{id}");
            }
            catch
            {
                return null;
            }
        }

        public static async Task<(bool Success, string Message)> SaveVendorAsync(VendorModel vendor)
        {
            try
            {
                var res = await http.PostAsJsonAsync("/api/admin/vendors", vendor);
                if (res.IsSuccessStatusCode)
                    return (true, string.Empty);

                var err = await res.Content.ReadAsStringAsync();
                return (false, $"Status {res.StatusCode}: {err}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<bool> DeleteVendorAsync(string docId)
        {
            try
            {
                var res = await http.DeleteAsync($"/api/admin/vendors/{docId}");
                return res.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ================= CITY METHODS =================

        public static async Task<List<CityModel>> GetCitiesAsync()
        {
            try
            {
                return await http.GetFromJsonAsync<List<CityModel>>("/api/admin/cities") ?? new List<CityModel>();
            }
            catch
            {
                return new List<CityModel>();
            }
        }

        public static async Task<CityModel> GetCityByIdAsync(string id)
        {
            try
            {
                return await http.GetFromJsonAsync<CityModel>($"/api/admin/cities/{id}");
            }
            catch
            {
                return null;
            }
        }

        public static async Task<(bool Success, string Message)> SaveCityAsync(CityModel city)
        {
            try
            {
                var res = await http.PostAsJsonAsync("/api/admin/cities", city);
                if (res.IsSuccessStatusCode)
                    return (true, string.Empty);

                var err = await res.Content.ReadAsStringAsync();
                return (false, $"Status {res.StatusCode}: {err}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<bool> DeleteCityAsync(string id)
        {
            try
            {
                var res = await http.DeleteAsync($"/api/admin/cities/{id}");
                return res.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // External Cities Sync
        public static async Task<(bool Success, string Message)> SyncCitiesFromExternalAsync(string url)
        {
            try
            {
                var encodedUrl = Uri.EscapeDataString(url);
                var res = await http.PostAsync($"/api/admin/cities/sync-external?url={encodedUrl}", null);
                var content = await res.Content.ReadAsStringAsync();

                if (res.IsSuccessStatusCode)
                {
                    return (true, content);
                }
                return (false, $"Error {res.StatusCode}: {content}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // ================= DEPOT METHODS =================

        public static async Task<List<DepotData>> GetDepotsAsync()
        {
            try
            {
                return await http.GetFromJsonAsync<List<DepotData>>("/api/admin/depots") ?? new List<DepotData>();
            }
            catch
            {
                return new List<DepotData>();
            }
        }

        public static async Task<DepotData> GetDepotByIdAsync(string id)
        {
            try
            {
                return await http.GetFromJsonAsync<DepotData>($"/api/admin/depots/{id}");
            }
            catch
            {
                return null;
            }
        }

        public static async Task<(bool Success, string Message)> SaveDepotAsync(DepotData depot)
        {
            try
            {
                var res = await http.PostAsJsonAsync("/api/admin/depots", depot);
                if (res.IsSuccessStatusCode)
                    return (true, string.Empty);

                var err = await res.Content.ReadAsStringAsync();
                return (false, $"Status {res.StatusCode}: {err}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<bool> DeleteDepotAsync(string id)
        {
            try
            {
                var res = await http.DeleteAsync($"/api/admin/depots/{id}");
                return res.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ================= BUS TYPE METHODS =================

        public static async Task<List<BusTypeModel>> GetBusTypesAsync()
        {
            try
            {
                return await http.GetFromJsonAsync<List<BusTypeModel>>("/api/admin/bustypes") ?? new List<BusTypeModel>();
            }
            catch
            {
                return new List<BusTypeModel>();
            }
        }

        public static async Task<BusTypeModel> GetBusTypeByIdAsync(string id)
        {
            try
            {
                return await http.GetFromJsonAsync<BusTypeModel>($"/api/admin/bustypes/{id}");
            }
            catch
            {
                return null;
            }
        }

        public static async Task<(bool Success, string Message)> SaveBusTypeAsync(BusTypeModel busType)
        {
            try
            {
                var res = await http.PostAsJsonAsync("/api/admin/bustypes", busType);
                if (res.IsSuccessStatusCode)
                    return (true, string.Empty);

                var err = await res.Content.ReadAsStringAsync();
                return (false, $"Status {res.StatusCode}: {err}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<bool> DeleteBusTypeAsync(string id)
        {
            try
            {
                var res = await http.DeleteAsync($"/api/admin/bustypes/{id}");
                return res.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ================= BUS METHODS =================

        public static async Task<List<BusData>> GetBusesAsync()
        {
            try
            {
                return await http.GetFromJsonAsync<List<BusData>>("/api/admin/buses") ?? new List<BusData>();
            }
            catch
            {
                return new List<BusData>();
            }
        }

        public static async Task<(bool Success, List<BusData> Buses, string Message)> GetBusesForDuplicateCheckAsync()
        {
            try
            {
                var response = await http.GetAsync("/api/admin/buses");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return (false, null, $"Status {response.StatusCode}: {error}");
                }

                var buses = await response.Content.ReadFromJsonAsync<List<BusData>>();

                if (buses == null)
                {
                    return (false, null, "Server se bus data nahi mila.");
                }

                return (true, buses, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        public static async Task<BusData> GetBusByIdAsync(string id)
        {
            try
            {
                return await http.GetFromJsonAsync<BusData>($"/api/admin/buses/{id}");
            }
            catch
            {
                return null;
            }
        }

        public static async Task<(bool Success, string Message)> SaveBusAsync(BusData bus, bool isNew)
        {
            try
            {
                HttpResponseMessage res = isNew
                    ? await http.PostAsJsonAsync("/api/admin/buses", bus)
                    : await http.PutAsJsonAsync($"/api/admin/buses/{bus.id}", bus);

                if (res.IsSuccessStatusCode)
                    return (true, string.Empty);

                var err = await res.Content.ReadAsStringAsync();
                return (false, $"Status {res.StatusCode}: {err}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<bool> DeleteBusAsync(string id)
        {
            try
            {
                var res = await http.DeleteAsync($"/api/admin/buses/{id}");
                return res.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ================= METADATA (Dropdowns) =================

        public static async Task<MetadataResponse> GetMetadataAsync(bool forceRefresh = false)
        {
            // Agar pehle se memory me hai aur force refresh nahi maanga, toh 0ms me return karo
            if (IsMetadataLoaded && !forceRefresh)
            {
                return MetadataCache;
            }

            try
            {
                var meta = await http.GetFromJsonAsync<MetadataResponse>("/api/admin/meta");
                if (meta != null)
                {
                    lock (MetadataCache)
                    {
                        MetadataCache = meta;
                        IsMetadataLoaded = true;
                    }
                }
            }
            catch
            {
                if (!IsMetadataLoaded) MetadataCache = new MetadataResponse();
            }

            return MetadataCache;
        }

        // ================= AUTH & USER MANAGEMENT =================

        public static async Task<(bool Success, string Message, string Token, bool MustChangePassword)> LoginAsync(string username, string password)
        {
            try
            {
                var payload = new { username = username, password = password };
                var res = await http.PostAsJsonAsync("/api/auth/login", payload);

                if (res.IsSuccessStatusCode)
                {
                    var result = await res.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result != null && !string.IsNullOrWhiteSpace(result.token))
                    {
                        SetAuthToken(result.token);
                        CurrentUsername = username;
                        CurrentDisplayName = username;

                        // Save session locally
                        SessionManager.SaveSession(result.token, username, CurrentDisplayName);

                        return (true, "Success", result.token, result.must_change_password);
                    }
                }

                var err = await res.Content.ReadAsStringAsync();
                return (false, "Invalid username or password.", string.Empty, false);
            }
            catch (Exception ex)
            {
                return (false, $"Connection error: {ex.Message}", string.Empty, false);
            }
        }

        public static async Task<(bool Success, string Message)> ChangePasswordAsync(string username, string oldPassword, string newPassword)
        {
            try
            {
                var payload = new
                {
                    username = username,
                    oldPassword = oldPassword,
                    newPassword = newPassword
                };

                var res = await http.PostAsJsonAsync("/api/auth/change-password", payload);
                var resContent = await res.Content.ReadAsStringAsync();

                if (res.IsSuccessStatusCode)
                {
                    return (true, "Password successfully updated!");
                }

                return (false, resContent);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<(bool Success, string Message)> RequestPasswordResetAsync(string username)
        {
            try
            {
                var payload = new { username = username };
                var res = await http.PostAsJsonAsync("/api/auth/forgot-password", payload);

                if (res.IsSuccessStatusCode)
                {
                    return (true, "Password reset request server par submit ho gayi hai.\nSuper Admin se temporary password prapt karein.");
                }

                var err = await res.Content.ReadAsStringAsync();
                return (false, err);
            }
            catch (Exception ex)
            {
                return (false, $"Connection error: {ex.Message}");
            }
        }

        public static async Task<bool> VerifySavedSessionAsync()
        {
            var session = SessionManager.LoadSession();
            if (session == null || string.IsNullOrWhiteSpace(session.Token))
                return false;

            try
            {
                SetAuthToken(session.Token);
                var res = await http.GetAsync("/api/auth/verify");

                if (res.IsSuccessStatusCode)
                {
                    CurrentUsername = session.Username;
                    CurrentDisplayName = session.DisplayName;
                    return true;
                }

                // Token invalid ya expired
                SessionManager.ClearSession();
                SetAuthToken(string.Empty);
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<List<BusData>> GetCachedBusesAsync(bool forceRefresh = false)
        {
            if (IsBusCacheLoaded && !forceRefresh)
            {
                return BusCache;
            }

            try
            {
                var buses = await GetBusesAsync();
                BusCache = buses ?? new List<BusData>();
                IsBusCacheLoaded = true;
            }
            catch
            {
                if (!IsBusCacheLoaded) BusCache = new List<BusData>();
            }

            return BusCache;
        }

        public static void SetupBusCacheSocketSync()
        {
            if (Socket == null) return;

            // 1. Live Bus Created
            Socket.On("bus_created", response =>
            {
                try
                {
                    var newBus = response.GetValue<BusData>(0);
                    if (newBus != null)
                    {
                        lock (BusCache)
                        {
                            BusCache.RemoveAll(b => b.id == newBus.id);
                            BusCache.Insert(0, newBus);
                        }
                    }
                }
                catch { }
                return Task.CompletedTask;
            });

            // 2. Live Bus Updated
            Socket.On("bus_updated", response =>
            {
                try
                {
                    var updatedBus = response.GetValue<BusData>(0);
                    if (updatedBus != null)
                    {
                        lock (BusCache)
                        {
                            int index = BusCache.FindIndex(b => b.id == updatedBus.id);
                            if (index >= 0)
                            {
                                BusCache[index] = updatedBus;
                            }
                            else
                            {
                                BusCache.Insert(0, updatedBus);
                            }
                        }
                    }
                }
                catch { }
                return Task.CompletedTask;
            });

            // 3. Live Bus Deleted
            Socket.On("bus_deleted", response =>
            {
                try
                {
                    var payload = response.GetValue<BusData>(0);
                    string? idToDelete = payload?.id;

                    if (!string.IsNullOrEmpty(idToDelete))
                    {
                        lock (BusCache)
                        {
                            BusCache.RemoveAll(b => b.id == idToDelete);
                        }
                    }
                }
                catch { }
                return Task.CompletedTask;
            });
        }

        public static void SetupMetadataSocketSync()
        {
            if (Socket == null) return;

            // ================= 1. CITIES SYNC =================
            Socket.On("city_created", context =>
            {
                try
                {
                    var city = context.GetValue<CityModel>(0);
                    if (city != null && !string.IsNullOrWhiteSpace(city.CityName))
                    {
                        lock (MetadataCache)
                        {
                            if (!MetadataCache.cities.Contains(city.CityName))
                            {
                                MetadataCache.cities.Add(city.CityName);
                                MetadataCache.cities.Sort();
                            }
                        }
                    }
                }
                catch { }
                return Task.CompletedTask;
            });

            // Rename/Edit hone par dropdown cache soft-sync
            Socket.On("city_updated", context =>
            {
                try { _ = GetMetadataAsync(forceRefresh: true); } catch { }
                return Task.CompletedTask;
            });

            Socket.On("city_deleted", context =>
            {
                try { _ = GetMetadataAsync(forceRefresh: true); } catch { }
                return Task.CompletedTask;
            });


            // ================= 2. VENDORS SYNC =================
            Socket.On("vendor_created", context =>
            {
                try
                {
                    var vendor = context.GetValue<VendorModel>(0);
                    if (vendor != null && !string.IsNullOrWhiteSpace(vendor.VendorName))
                    {
                        lock (MetadataCache)
                        {
                            if (!MetadataCache.vendors.Contains(vendor.VendorName))
                            {
                                MetadataCache.vendors.Add(vendor.VendorName);
                                MetadataCache.vendors.Sort();
                            }
                        }
                    }
                }
                catch { }
                return Task.CompletedTask;
            });

            Socket.On("vendor_updated", context =>
            {
                try { _ = GetMetadataAsync(forceRefresh: true); } catch { }
                return Task.CompletedTask;
            });

            Socket.On("vendor_deleted", context =>
            {
                try { _ = GetMetadataAsync(forceRefresh: true); } catch { }
                return Task.CompletedTask;
            });


            // ================= 3. DEPOTS SYNC =================
            Socket.On("depot_created", context =>
            {
                try
                {
                    var depot = context.GetValue<DepotData>(0);
                    if (depot != null && !string.IsNullOrWhiteSpace(depot.Depot))
                    {
                        lock (MetadataCache)
                        {
                            if (!MetadataCache.depots.Contains(depot.Depot))
                            {
                                MetadataCache.depots.Add(depot.Depot);
                                MetadataCache.depots.Sort();
                            }
                        }
                    }
                }
                catch { }
                return Task.CompletedTask;
            });

            Socket.On("depot_updated", context =>
            {
                try { _ = GetMetadataAsync(forceRefresh: true); } catch { }
                return Task.CompletedTask;
            });

            Socket.On("depot_deleted", context =>
            {
                try { _ = GetMetadataAsync(forceRefresh: true); } catch { }
                return Task.CompletedTask;
            });


            // ================= 4. BUS TYPES SYNC =================
            Socket.On("bustype_created", context =>
            {
                try
                {
                    var bt = context.GetValue<BusTypeModel>(0);
                    if (bt != null && !string.IsNullOrWhiteSpace(bt.BusType))
                    {
                        lock (MetadataCache)
                        {
                            if (!MetadataCache.busTypes.Contains(bt.BusType))
                            {
                                MetadataCache.busTypes.Add(bt.BusType);
                                MetadataCache.busTypes.Sort();
                            }
                        }
                    }
                }
                catch { }
                return Task.CompletedTask;
            });

            Socket.On("bustype_updated", context =>
            {
                try { _ = GetMetadataAsync(forceRefresh: true); } catch { }
                return Task.CompletedTask;
            });

            Socket.On("bustype_deleted", context =>
            {
                try { _ = GetMetadataAsync(forceRefresh: true); } catch { }
                return Task.CompletedTask;
            });
        }

        public static void Logout()
        {
            SessionManager.ClearSession();
            SetAuthToken(string.Empty);
            CurrentUsername = string.Empty;
            CurrentDisplayName = string.Empty;
        }

        public class MetadataResponse
        {
            public List<string> cities { get; set; } = new List<string>();
            public List<string> vendors { get; set; } = new List<string>();
            public List<string> busTypes { get; set; } = new List<string>();
            public List<string> depots { get; set; } = new List<string>();
        }
    }

    public class DashboardCounts
    {
        public int CityInfo { get; set; }
        public int VendorInfo { get; set; }
        public int BusInfo { get; set; }
        public int Depots { get; set; }
    }

    public class LoginResponse
    {
        public bool success { get; set; }
        public string token { get; set; }
        public bool must_change_password { get; set; }
    }
}