namespace Domain.Shared;

public class AuctionInfo
{
    public long Id { get; set; }
    public List<VehicleInfo> Vehicles { get; set; } = [];
    
}