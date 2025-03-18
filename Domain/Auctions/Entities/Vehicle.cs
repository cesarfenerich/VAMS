using Domain.Shared;

namespace Domain.Auctions;

internal record Vehicle
{
    public long Id { get; }
    public VehicleTypes Type { get; }
    public string Manufacturer { get; }
    public string Model { get; }
    public int Year { get; }
    public decimal StartingBid { get; }
    public decimal? WinnerBid { get; private set; }
    public VehicleStatuses Status { get; private set; }
    public List<Bid> Bids { get; private set; } = [];   

    public Vehicle(long id, 
                   VehicleTypes type, 
                   string manufacturer, 
                   string model, 
                   int year,  
                   decimal startingBid)
    {
        Id = id;
        Type = type;
        Manufacturer = manufacturer;
        Model = model;
        Year = year;
        StartingBid = startingBid;
        Status = VehicleStatuses.InAuction;
    }

    public void PlaceBid(decimal amount)
    {
        Bids.Add(new Bid(amount, StartingBid, Bids));
    }

    public void SellOrRelease() 
    {
        if(Bids.Count == 0 && WinnerBid is null)
            Status = VehicleStatuses.Available;
        else
        {
            Status = VehicleStatuses.Sold;
            WinnerBid = Bids.Max(x => x.Amount);
        }       
    }   

    public AuctionVehicleInfo AsModel()
    {
        return new AuctionVehicleInfo()
        {
            Id = Id,
            Type = Type,
            Manufacturer = Manufacturer,
            Model = Model,
            Year = Year,
            StartingBid = StartingBid,
            WinnerBid = WinnerBid,
            Bids = Bids.Select(x => x.AsModel(WinnerBid ?? 0)).ToList(),
            Status = Status
        };
    }
}
