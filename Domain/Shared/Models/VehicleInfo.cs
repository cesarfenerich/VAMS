﻿namespace Domain.Shared;

public class VehicleInfo
{
    public long Id { get; set; }
    public VehicleTypes Type { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal StartingBid { get; set; }
    public VehicleStatuses Status { get; set; }
    public int? NumberOfDoors { get; set; }
    public int? NumberOfSeats { get; set; }
    public double? LoadCapacity { get; set; }
}