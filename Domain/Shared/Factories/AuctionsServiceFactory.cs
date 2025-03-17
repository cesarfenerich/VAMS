using Domain.Auctions;

namespace Domain.Shared;

public static class AuctionsServiceFactory
{  
    public static AuctionsService CreateAuctionsService(IVehiclesService vehiclesService, 
                                                        IVehiclesHandler vehiclesHandler)
    {
        return new AuctionsService(vehiclesService, vehiclesHandler);
    }

    public static IAuctionsHandler CreateAuctionsHandler(AuctionsService auctionsService)
    {
        return new AuctionsHandler(auctionsService);
    }
}