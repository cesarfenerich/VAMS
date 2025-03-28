﻿namespace Domain.Shared;

public class AuctionVehicleInfo
{
    public long Id { get; set; }
    public VehicleTypes Type { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal StartingBid { get; set; }
    public decimal? WinnerBid { get; set; }
    public List<BidInfo> Bids { get; set; } = [];
    public VehicleStatuses Status { get; set; }    
}