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

    public AuctionInfo StartAuction(StartAuction command)
    {
        if (command.VehicleIds is null || command.VehicleIds.Count == 0)
            throw new AuctionsException("Auction must have at least one vehicle.");       

        var lastId = _inventory.Count != 0 ? _inventory.Max(act => act.Id) : 1;

        return new Auction(lastId, ValidateVehiclesForAuction(command), command.EndDate).AsModel();
    }

    private List<Vehicle> ValidateVehiclesForAuction(StartAuction command)
    {
        var availableVehicles = new List<Vehicle>();
        foreach (var id in command.VehicleIds)
        {
            var vehicle = _vehiclesService.GetVehicleById(id);           

            var isAlreadyInAuction = _inventory.Where(x => x.Status == AuctionStatuses.Open)
                                               .Any(x => x.Vehicles.Any(y => y.Id == id));

            if (isAlreadyInAuction || vehicle.Status != VehicleStatuses.Available)
                throw new AuctionsException($"Vehicle with id ({id}) is not available for auction.");

            availableVehicles.Add(new Vehicle(vehicle.Id, vehicle.StartingBid, vehicle.Status));
        }

        return availableVehicles;
    }   

    public AuctionInfo EndAuction(EndAuction command)
    {
        var auction = _inventory.FirstOrDefault(x => x.Id == command.AuctionId) ?? 
                      throw new AuctionsException($"Auction {command.AuctionId} was not found.");

        auction.Close(ref _inventory);

        UpdateVehiclesStatus upCommand = new()
        {
            VehicleIds = [.. auction.Vehicles.Where(x => x.Status == VehicleStatuses.Sold).Select(y => y.Id)],
            Status = VehicleStatuses.Sold
        };

        _vehiclesService.UpdateVehiclesStatus(upCommand);      

        return auction.AsModel();
    }   
}
