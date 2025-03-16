namespace Domain.Shared;

public class UpdateVehiclesByAuction
{
    public long AuctionId { get; set; }
    public List<long> VehiclesToBeUpdated { get; set; } = [];    
}