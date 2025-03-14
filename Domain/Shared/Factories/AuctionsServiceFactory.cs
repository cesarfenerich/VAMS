using Domain.Auctions;

namespace Domain.Shared;

public static class AuctionsServiceFactory
{
    public static IAuctionsService CreateAuctionsService()
    {
        return new AuctionsService();
    }
}