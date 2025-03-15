using Domain.Shared;

namespace Domain.Auctions;

internal record Auction
{
    public long Id { get; }

    public IReadOnlyCollection<Vehicle> Vehicles { get; private set; }

    public DateTime EndDate { get; private set; }

    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }   

    public AuctionStatuses Status => EndedAt is not null ? AuctionStatuses.Closed : AuctionStatuses.Open;

    public Auction(long lastId, List<Vehicle> vehicles, DateTime endDate)
    {
        Id = ++lastId;
        StartedAt = DateTime.UtcNow;
        Vehicles = vehicles is null || vehicles.Count == 0 ? throw new AuctionsException("Auction must have at least one vehicle.") : vehicles;
        EndDate = endDate == default ? throw new VehiclesException("EndDate is required.") : endDate;
    }    

    public void PlaceBid(Bid bid)
    {
        if (EndedAt is not null)
            throw new AuctionsException("Cannot place a bid because the auction already ended.");

        var vehicle = Vehicles.FirstOrDefault(x => x.Id == bid.VehicleId);

        if(vehicle is null)
            throw new AuctionsException("Vehicle not available on this auction.");

       //TODO: Finish to check the bid amount and other stuff

    }
    public void Close()
    {
        EndedAt = DateTime.UtcNow;
    }

    public AuctionInfo AsModel()
    {
        return new AuctionInfo()
        {
            Id = Id,
            Vehicles = [.. Vehicles.Select(x => x.AsModel())],
            StartDate = StartedAt,
            EndDate = EndDate,
            Status = Status            
        };
    }
} 

