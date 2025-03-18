using Domain.Shared;

namespace Domain.Vehicles;

internal class VehiclesHandler(IVehiclesCommandService vehiclesCommandService) : IVehiclesHandler
{
    private readonly IVehiclesCommandService _vehiclesService = vehiclesCommandService;

    public void Handle(AddVehicle command)
    {
        _vehiclesService.AddVehicle(command);
    }

    public void Handle(UpdateVehiclesByAuction command)
    {
        _vehiclesService.UpdateInventoryByAuction(command);
    }
}
