namespace Domain.Shared;

public interface IAuctionsService
{       
    AuctionInfo GetAuctionById(long id);
    AuctionsView GetAuctions();
}
