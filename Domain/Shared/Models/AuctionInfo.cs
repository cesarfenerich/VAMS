namespace Domain.Shared;

public class AuctionInfo
{
    public long Id { get; set; }
    public List<AuctionVehicleInfo> Vehicles { get; set; } = [];
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public AuctionStatuses Status { get; set; }
}