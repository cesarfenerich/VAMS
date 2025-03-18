using Domain.Shared;

namespace Domain.Auctions;

internal record Auction
{
    public long Id { get; }
    public DateTime Start{ get; private set; }
    public DateTime End { get; private set; }
    public AuctionStatuses Status { get; private set; }
    public IReadOnlyCollection<Vehicle> Vehicles { get; private set; }    

    public Auction(long lastId, List<Vehicle> vehicles, DateTime endDate)
    {
        Id = ++lastId;
        Start = DateTime.UtcNow;
        Status = AuctionStatuses.Created;
        Vehicles = vehicles;
        End = endDate == default || endDate <= DateTime.UtcNow ? throw new AuctionsException("Invalid auction end date.") : endDate;
    }

    public void Open()
    {
        if (Status == AuctionStatuses.Open)
            throw new AuctionsException("Auction was already open.");

        if (Status == AuctionStatuses.Closed)
            throw new AuctionsException("An auction cannot be reopened.");             

        Status = AuctionStatuses.Open;        
    }

    public void PlaceBid(long vehicleId, decimal amount)
    {
        var vhc = Vehicles.FirstOrDefault(x => x.Id == vehicleId) ??
          throw new AuctionsException("Vehicle not found.");

        if (Status != AuctionStatuses.Open)
            throw new AuctionsException("Auction is not open for bids.");      

        vhc.PlaceBid(amount);  
    }

    public void Close()
    {
        if (DateTime.UtcNow < End)
            throw new AuctionsException("Cannot close the auction before the end date.");

        if (Status == AuctionStatuses.Closed)
            throw new AuctionsException("Auction was already closed.");

        Vehicles.ToList().ForEach(vhc => vhc.SellOrRelease());

        Status = AuctionStatuses.Closed;       
    }

    public AuctionInfo AsModel()
    {
        return new AuctionInfo()
        {
            Id = Id,
            Vehicles = Vehicles.Select(x => x.AsModel()).ToList(),
            Start = Start,
            End = End,
            Status = Status            
        };
    }
} 

