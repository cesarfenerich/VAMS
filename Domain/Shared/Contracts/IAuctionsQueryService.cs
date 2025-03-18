namespace Domain.Shared;

public interface IAuctionsQueryService
{       
    AuctionInfo GetAuctionById(long id);
    AuctionsView GetAuctions();
}
