using Domain.Shared;

namespace Domain.Auctions;

internal record Auction
{
    public long Id { get; }

    public IReadOnlyCollection<Vehicle> Vehicles { get; private set; }  

    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }   

    public Auction(long lastId, List<Vehicle> vehicles)
    {
        Id = ++lastId;
        StartedAt = DateTime.UtcNow;
        Vehicles = vehicles;
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
} 

