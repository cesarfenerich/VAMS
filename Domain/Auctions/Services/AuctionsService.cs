using Domain.Shared;

namespace Domain.Auctions;

public class AuctionsService(IVehiclesService vehiclesService, 
                             IVehiclesHandler vehiclesHandler) : IAuctionsService
{
    private readonly List<Auction> _inventory = []; //Just for brevity, this should be a repository/other data access mechanism
    private readonly IVehiclesService _vehiclesService = vehiclesService;
    private readonly IVehiclesHandler _vehiclesHandler = vehiclesHandler;

    #region Commands

    internal void StartAuction(StartAuction command)
    {
        var lastId = _inventory.Count == 0 ? 0 : _inventory.Max(x => x.Id);

        var auction = new Auction(lastId,
                                  ValidateVehiclesForAuction(command),
                                  command.EndDate);
        auction.Open();
        _inventory.Add(auction);

        _vehiclesHandler.Handle(new UpdateVehiclesByAuction(auction.Id));
    }

    internal void PlaceBid(PlaceBid command)
    {
        var auction = _inventory.FirstOrDefault(x => x.Id == command.AuctionId) ??
                      throw new AuctionsException($"Auction {command.AuctionId} was not found.");

        auction.PlaceBid(command.VehicleId, command.Amount);  
    }

    internal void EndAuction(EndAuction command)
    {
        var auction = _inventory.FirstOrDefault(x => x.Id == command.AuctionId) ??
                      throw new AuctionsException($"Auction {command.AuctionId} was not found.");

        _inventory.Remove(auction);
        auction.Close();
        _inventory.Add(auction);

        _vehiclesHandler.Handle(new UpdateVehiclesByAuction(auction.Id));      
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

    #endregion

    #region Queries

    public AuctionInfo GetAuctionById(long id)
    {
        var auction = _inventory.FirstOrDefault(acn => acn.Id == id)?.AsModel();

        return auction ?? throw new AuctionsException($"Auction with id {id} not found.");
    }

    public AuctionsView GetAuctions()
        => new([.. _inventory.Select(x => x.AsModel())]);

    #endregion
}
