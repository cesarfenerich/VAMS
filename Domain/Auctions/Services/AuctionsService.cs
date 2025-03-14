using Domain.Shared;

namespace Domain.Auctions;

internal class AuctionsService : IAuctionsService
{
    IVehiclesService _vehiclesService;

    public AuctionsService()
    {

            
    } 

    public AuctionInfo StartAuction(StartAuction command)
    {
        throw new NotImplementedException();
    }

    public AuctionInfo EndAuction(EndAuction command)
    {
        throw new NotImplementedException();
    }

    public BidInfo? PlaceBid(PlaceBid command)
    {
        throw new NotImplementedException();
    }
}
