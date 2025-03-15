using Domain.Shared;

namespace Domain.Auctions;

internal record Vehicle
{
    public long Id { get; }

    public decimal StartingBid { get; }

    public VehicleStatuses Status { get; }

    public Vehicle(long id, decimal startingBid, VehicleStatuses status)
    {
        Id = id;       
        StartingBid = startingBid;
        Status = status;
    }

    public VehicleInfo AsModel()
    {
        return new VehicleInfo()
        {
            Id = Id,
            StartingBid = StartingBid,
            Status = Status
        };
    }
}
