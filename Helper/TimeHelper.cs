using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Businfo.Helper
{
    public class TimeHelper
    {
        public static string SerializeTime(string time)
        {
            if (string.IsNullOrWhiteSpace(time))
                return "00:00";

            try
            {
                // Split by ':' if present
                var parts = time.Split(':');

                int hours = 0;
                int minutes = 0;

                if (parts.Length == 1)
                {
                    // Sirf hours diye
                    hours = int.TryParse(parts[0], out var h) ? h : 0;
                }
                else if (parts.Length >= 2)
                {
                    hours = int.TryParse(parts[0], out var h) ? h : 0;
                    minutes = int.TryParse(parts[1], out var m) ? m : 0;
                }

                // Normalize minutes > 59
                hours += minutes / 60;
                minutes = minutes % 60;

                // 24-hour wrap
                hours = hours % 24;

                // Return HH:mm format
                return $"{hours:D2}:{minutes:D2}";
            }
            catch
            {
                return "00:00"; // fallback
            }
        }
        public static string SerializeMultipleTimes(string timesInput)
        {
            if (string.IsNullOrWhiteSpace(timesInput))
                return "00:00";

            // Split by comma
            var times = timesInput.Split(',');

            // Serialize each time individually
            var serializedTimes = times.Select(t => SerializeTime(t.Trim()));

            // Join back with comma
            return string.Join(",", serializedTimes);
        }
    }
}
