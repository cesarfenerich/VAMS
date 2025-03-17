using Domain.Vehicles;

namespace Domain.Shared;

public static class VehiclesServiceFactory
{
    public static VehiclesService CreateVehiclesService(IAuctionsService auctionsService)
    {
        return new VehiclesService(auctionsService);
    }

    public static IVehiclesHandler CreateVehiclesHandler(IVehiclesService vehiclesService)
    {
        return new VehiclesHandler((VehiclesService)vehiclesService);
    }
}