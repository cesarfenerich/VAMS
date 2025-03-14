using Domain.Shared;

namespace Domain.Auctions;

internal record Vehicle
{
    public long Id { get; }

    public long AuctionId { get; }

    public VehicleStatuses Status { get; set; }

    public decimal StartingBid { get; }    

}
