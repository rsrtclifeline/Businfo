
using Businfo.Manage;
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

        static ApiClient()
        {
            http.BaseAddress = new Uri(BaseUrl);
            http.DefaultRequestHeaders.Add("x-admin-key", "RSRTC_ADMIN_SUPER_SECRET_2026");
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

        // Safe method for duplicate checking
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

        public static async Task<MetadataResponse> GetMetadataAsync()
        {
            try
            {
                return await http.GetFromJsonAsync<MetadataResponse>("/api/admin/meta") ?? new MetadataResponse();
            }
            catch
            {
                return new MetadataResponse();
            }
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
}
