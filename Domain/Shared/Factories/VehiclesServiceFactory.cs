using Domain.Vehicles;

namespace Domain.Shared;

public static class VehiclesServiceFactory
{
    public static IVehiclesService CreateVehiclesService()
    {
        return new VehiclesService();
    }
}