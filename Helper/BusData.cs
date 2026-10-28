using Businfo.AddEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

public class BusData
{
    [JsonPropertyName("id")]
    public string id { get; set; }

    [JsonPropertyName("Line")]
    public string Line { get; set; }

    [JsonPropertyName("From")]
    public string From { get; set; }

    [JsonPropertyName("To")]
    public string To { get; set; }

    [JsonPropertyName("Route")]
    public string Route { get; set; }

    [JsonPropertyName("Vendor")]
    public string Vendor { get; set; }

    [JsonPropertyName("BusType")]
    public string BusType { get; set; }

    [JsonPropertyName("Depot")]
    public string Depot { get; set; }

    [JsonPropertyName("Time")]
    public string Time { get; set; }

    [JsonIgnore]
    public string Departure
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Time)) return "";
            var parts = Time.Split(',');
            return parts.Length > 0 ? parts[0] : "";
        }
    }

    [JsonIgnore]
    public string Arrival
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Time)) return "";
            var parts = Time.Split(',');
            return parts.Length > 1 ? parts[1] : "";
        }
    }

    [JsonPropertyName("Stops")]
    public List<StopData> Stops { get; set; } = new List<StopData>();

    [JsonPropertyName("GUID")]
    public string GUID { get; set; } = Guid.NewGuid().ToString();

    public BusData Clone()
    {
        return new BusData
        {
            id = this.id,
            Line = this.Line,
            From = this.From,
            To = this.To,
            Route = this.Route,
            Vendor = this.Vendor,
            BusType = this.BusType,
            Depot = this.Depot,
            Time = this.Time,
            Stops = this.Stops != null ? this.Stops.Select(s => s.Clone()).ToList() : new List<StopData>(),
            GUID = Guid.NewGuid().ToString()
        };
    }
}