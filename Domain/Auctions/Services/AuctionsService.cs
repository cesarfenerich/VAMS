using Domain.Shared;

namespace Domain.Auctions;

internal class AuctionsService : IAuctionsService
{
    private List<Auction> _inventory = [];

    IVehiclesService _vehiclesService;

    public AuctionsService(IVehiclesService vehiclesService)
    {
        _vehiclesService = vehiclesService;
    }

    public AuctionInfo GetAuctionById(long id)
    {
        var auction = _inventory.FirstOrDefault(acn => acn.Id == id)?.AsModel();

        return auction ?? throw new AuctionsException($"Auction with id {id} not found.");
    }

    public AuctionInfo StartAuction(StartAuction command)
    {
        var lastId = _inventory.Count != 0 ? _inventory.Max(act => act.Id) : 1;

        var auction = new Auction(lastId, 
                                  ValidateVehiclesForAuction(command), 
                                  command.EndDate);
        auction.Open();
        _inventory.Add(auction);

        _vehiclesService.UpdateInventoryByAuction(auction.Id);

        return auction.AsModel();
    }
    public AuctionInfo PlaceBid(PlaceBid command)
    {
        var auction = _inventory.FirstOrDefault(x => x.Id == command.AuctionId) ??
                      throw new AuctionsException($"Auction {command.AuctionId} was not found.");        

        auction.PlaceBid(command.VehicleId, command.Amount);

        return auction.AsModel();
    }

    public AuctionInfo EndAuction(long auctionId)
    {
        var auction = _inventory.FirstOrDefault(x => x.Id == auctionId) ?? 
                      throw new AuctionsException($"Auction {auctionId} was not found.");   

        _inventory.Remove(auction);
        auction.Close();
        _inventory.Add(auction);       

        _vehiclesService.UpdateInventoryByAuction(auction.Id);      

        return auction.AsModel();
    }

    private List<Vehicle> ValidateVehiclesForAuction(StartAuction command)
    {
        var availableVehicles = new List<Vehicle>();
        var vehicles = _vehiclesService.GetAvailableVehicles().Vehicles.ToList();

        if (vehicles.Count == 0)
            throw new AuctionsException($"There is no vehicle to be auctioned.");

        var vehicleIds = vehicles.Select(x => x.Id).ToList();

        //If it's missing any from the informed available vehicles.
        if (command.VehicleIds.Any(x => !vehicleIds.Any(y => x == y)))        
            throw new AuctionsException($"One or more vehicles are not available to be auctioned.");

        vehicles.ForEach(vhc => availableVehicles.Add(new Vehicle(vhc.Id, 
                                                                  vhc.Type,
                                                                  vhc.Manufacturer,
                                                                  vhc.Model,
                                                                  vhc.Year,
                                                                  vhc.StartingBid)));
        return availableVehicles;
    }
}
