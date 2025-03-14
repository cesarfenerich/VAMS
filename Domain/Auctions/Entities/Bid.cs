namespace Domain.Auctions;

internal record Bid
{
    public long Id { get; private set; }

    public long VehicleId { get; set; }

    public decimal Amount { get; }

    public DateTime PlacedAt { get; }

}

