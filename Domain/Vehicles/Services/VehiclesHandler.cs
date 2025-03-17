using Domain.Shared;

namespace Domain.Vehicles;

internal class VehiclesHandler(VehiclesService vehiclesService) : IVehiclesHandler
{
    private readonly VehiclesService _vehiclesService = vehiclesService;

    public void Handle(AddVehicle command)
    {
        _vehiclesService.AddVehicle(command);
    }

    public void Handle(UpdateVehiclesByAuction command)
    {
        _vehiclesService.UpdateInventoryByAuction(command);
    }
}
