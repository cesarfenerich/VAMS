using Domain.Vehicles;

namespace Domain.Shared;

public static class VehiclesFactory
{
    public static IVehiclesRepository CreateVehiclesRepository()
        => new VehiclesRepository();

    public static IVehiclesQueryService CreateVehiclesQueryService(IVehiclesRepository repository)
        => new VehiclesQueryService(repository);

    public static IVehiclesCommandService CreateVehiclesCommandService(IVehiclesRepository vehiclesRepository, IAuctionsQueryService auctionsQueryService)
       => new VehiclesCommandService(vehiclesRepository, auctionsQueryService);       

    public static IVehiclesHandler CreateVehiclesHandler(IVehiclesCommandService vehiclesCommandService) 
        => new VehiclesHandler(vehiclesCommandService);
}