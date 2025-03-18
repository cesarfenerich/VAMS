using Domain.Shared;

namespace Domain.Auctions;

internal class AuctionsCommandService(IAuctionsRepository auctionsRepository,
                                      IVehiclesHandler vehiclesHandler, 
                                      IVehiclesQueryService vehiclesService) : IAuctionsCommandService
{
    private readonly IAuctionsRepository _auctions = auctionsRepository;
    private readonly IVehiclesQueryService _vehiclesService = vehiclesService;
    private readonly IVehiclesHandler _vehiclesHandler = vehiclesHandler;

    public void StartAuction(StartAuction command)
    {
        var lastId = _auctions.GetLastId();

        var auction = new Auction(lastId,
                                  ValidateVehiclesForAuction(command),
                                  command.EndDate);
        auction.Open();
        _auctions.Add(auction);

        _vehiclesHandler.Handle(new UpdateVehiclesByAuction(auction.Id));
    }

    public void PlaceBid(PlaceBid command)
    {
        var auction = _auctions.GetById(command.AuctionId) ??
                      throw new AuctionsException($"Auction {command.AuctionId} was not found.");

        auction.PlaceBid(command.VehicleId, command.Amount);
    }

    public void EndAuction(EndAuction command)
    {
        var auction = _auctions.GetById(command.AuctionId) ??
                      throw new AuctionsException($"Auction {command.AuctionId} was not found.");

        _auctions.Remove(auction);
        auction.Close();
        _auctions.Add(auction);

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
}
