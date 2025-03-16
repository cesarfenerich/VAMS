using Domain.Vehicles;

namespace Domain.Shared;

public static class VehiclesServiceFactory
{
    public static IVehiclesService CreateVehiclesService(IAuctionsService auctionsService)
    {
        return new VehiclesService(auctionsService);
    }
}