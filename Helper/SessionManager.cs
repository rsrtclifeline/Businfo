using System;
using System.IO;
using System.Text.Json;

namespace Businfo.Helper
{
    public class UserSession
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    public static class SessionManager
    {
        private static readonly string FolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Businfo");
        private static readonly string SessionFilePath = Path.Combine(FolderPath, "session.dat");

        public static void SaveSession(string token, string username, string displayName)
        {
            try
            {
                if (!Directory.Exists(FolderPath))
                    Directory.CreateDirectory(FolderPath);

                var session = new UserSession
                {
                    Token = token,
                    Username = username,
                    DisplayName = displayName
                };

                string json = JsonSerializer.Serialize(session);
                // Simple byte encoding taaki plain text JSON direct na dikhe
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
                File.WriteAllBytes(SessionFilePath, bytes);
            }
            catch { }
        }

        public static UserSession? LoadSession()
        {
            try
            {
                if (!File.Exists(SessionFilePath)) return null;

                byte[] bytes = File.ReadAllBytes(SessionFilePath);
                string json = System.Text.Encoding.UTF8.GetString(bytes);
                return JsonSerializer.Deserialize<UserSession>(json);
            }
            catch
            {
                return null;
            }
        }

        public static void ClearSession()
        {
            try
            {
                if (File.Exists(SessionFilePath))
                    File.Delete(SessionFilePath);
            }
            catch { }
        }
    }
}