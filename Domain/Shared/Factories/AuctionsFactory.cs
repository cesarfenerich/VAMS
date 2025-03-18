using Domain.Auctions;

namespace Domain.Shared;

public static class AuctionsFactory
{
    public static IAuctionsRepository CreateAuctionsRepository()
       => new AuctionsRepository();

    public static IAuctionsQueryService CreateAuctionsQueryService(IAuctionsRepository auctionsRepository)
        => new AuctionsQueryService(auctionsRepository);

    public static IAuctionsHandler CreateAuctionsHandler(IAuctionsCommandService auctionsCommandService)
        => new AuctionsHandler(auctionsCommandService);

    public static IAuctionsCommandService CreateAuctionsCommandService(IAuctionsRepository auctionsRepository,
                                                                       IVehiclesHandler vehiclesHandler,
                                                                       IVehiclesQueryService vehiclesQueryService)
    {
        return new AuctionsCommandService(auctionsRepository, vehiclesHandler, vehiclesQueryService);
    }    
}