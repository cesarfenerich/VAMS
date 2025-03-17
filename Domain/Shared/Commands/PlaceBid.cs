namespace Domain.Shared;

public class PlaceBid(long auctionId,
                      long vehicleId,
                      decimal amount) : Command
{
    public long AuctionId { get; } = auctionId;
    public long VehicleId { get; } = vehicleId;
    public decimal Amount { get; } = amount;

    public override bool IsValid()
    {
        //NICE_TO_HAVE: Implement command validations
        return true;
    }
}
