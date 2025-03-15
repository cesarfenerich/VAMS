namespace Domain.Shared;

public class AuctionInfo
{
    public long Id { get; set; }
    public List<VehicleInfo> Vehicles { get; set; } = [];
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public AuctionStatuses? Status { get; set; }

}