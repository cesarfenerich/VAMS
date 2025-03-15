using Domain.Shared;
namespace Domain.Auctions;

internal class AuctionsService : IAuctionsService
{
    private readonly List<Auction> _inventory = [];

    IVehiclesService _vehiclesService;

    public AuctionsService(IVehiclesService vehiclesService)
    {
        _vehiclesService = vehiclesService;
    } 

    public AuctionInfo StartAuction(StartAuction command)
    {
        if(command.VehicleIds is null || command.VehicleIds.Count == 0)
            throw new AuctionsException("Auction must have at least one vehicle.");

        var availableVehicles = new List<Vehicle>();
        foreach (var id in command.VehicleIds)
        {
            var vehicle = _vehiclesService.GetVehicleById(id);           

            if (vehicle.Status != VehicleStatuses.Available)
                throw new AuctionsException($"Vehicle with id {id} is not available for auction.");

            availableVehicles.Add(new Vehicle(vehicle.Id, vehicle.StartingBid, vehicle.Status));
        }

        var lastId = _inventory.Count != 0 ? _inventory.Max(act => act.Id) : 1;

        return new Auction(lastId, availableVehicles, command.EndDate).AsModel();
    }

    public AuctionInfo EndAuction(EndAuction command)
    {
        throw new NotImplementedException();
    }

    public BidInfo? PlaceBid(PlaceBid command)
    {
        throw new NotImplementedException();
    }
}
