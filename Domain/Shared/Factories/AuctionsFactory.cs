using Domain.Auctions;

namespace Domain.Shared;

public static class AuctionsFactory
{
    public static IAuctionsRepository CreateAuctionsRepository()
       => new AuctionsRepository();

    public static IAuctionsQueryService CreateAuctionsQueryService(IAuctionsRepository auctionsRepository)
        => new AuctionsQueryService(auctionsRepository);

    public static IAuctionsHandler CreateAuctionsHandler(IAuctionsRepository auctionsRepository,
                                                         IVehiclesHandler vehiclesHandler,
                                                         IVehiclesQueryService vehiclesQueryService)
    {
        return new AuctionsHandler(CreateAuctionsCommandService(auctionsRepository,
                                                                vehiclesHandler,
                                                                vehiclesQueryService));
    }

    private static IAuctionsCommandService CreateAuctionsCommandService(IAuctionsRepository auctionsRepository,
                                                                        IVehiclesHandler vehiclesHandler,
                                                                        IVehiclesQueryService vehiclesQueryService)
    {
        return new AuctionsCommandService(auctionsRepository, vehiclesHandler, vehiclesQueryService);
    }    
}