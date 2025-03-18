using Domain.Shared;

namespace Domain.Vehicles;

internal class VehiclesCommandService(IVehiclesRepository vehiclesRepository, IAuctionsQueryService auctionsService) : IVehiclesCommandService
{
    private readonly IVehiclesRepository _vehicles = vehiclesRepository;
    private readonly IAuctionsQueryService _auctionsQueryService = auctionsService;

    public void AddVehicle(AddVehicle command)
    {
        var lastId = _vehicles.GetLastId();

        var vehicle = new Vehicle(lastId,
                                  command.Type,
                                  command.Manufacturer,
                                  command.Model,
                                  command.Year,
                                  command.StartingBid,
                                  command.NumberOfDoors,
                                  command.NumberOfSeats,
                                  command.LoadCapacity);
        _vehicles.Add(vehicle);
    }

    public void UpdateInventoryByAuction(UpdateVehiclesByAuction command)
    {
        var auction = _auctionsQueryService.GetAuctionById(command.AuctionId);

        if (auction.Status == AuctionStatuses.Closed)
            throw new VehiclesException($"Cannot update because the auction ({auction.Id}) is closed.");

        var vhcIds = auction.Vehicles.Select(x => x.Id).ToList();
        var vhcs = _vehicles.GetByIds(vhcIds).ToList();

        vhcs.ForEach(vhc =>
        {
            var status = auction.Vehicles.First(x => x.Id == vhc.Id).Status;

            _vehicles.Remove(vhc);

            if (status == VehicleStatuses.InAuction)
                vhc.Reserve();
            else if (status == VehicleStatuses.Sold)
                vhc.Sell();
            else
                vhc.Release();

            _vehicles.Add(vhc);
        });
    }
}