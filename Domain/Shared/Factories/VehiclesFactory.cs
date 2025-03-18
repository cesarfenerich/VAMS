using Domain.Vehicles;

namespace Domain.Shared;

public static class VehiclesFactory
{
    public static IVehiclesRepository CreateVehiclesRepository()
        => new VehiclesRepository();

    public static IVehiclesQueryService CreateVehiclesQueryService(IVehiclesRepository repository)
        => new VehiclesQueryService(repository);      

    public static IVehiclesHandler CreateVehiclesHandler(IVehiclesRepository vehiclesRepository, IAuctionsQueryService auctionsQueryService) 
        => new VehiclesHandler(CreateVehiclesCommandService(vehiclesRepository, auctionsQueryService));

    private static IVehiclesCommandService CreateVehiclesCommandService(IVehiclesRepository vehiclesRepository, IAuctionsQueryService auctionsQueryService)
        => new VehiclesCommandService(vehiclesRepository, auctionsQueryService);
}