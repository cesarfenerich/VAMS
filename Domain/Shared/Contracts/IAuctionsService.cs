namespace Domain.Shared;

public interface IAuctionsService
{
    AuctionInfo GetAuctionById(long id);
    AuctionInfo StartAuction(StartAuction command);
    AuctionInfo PlaceBid(PlaceBid command);
    AuctionInfo EndAuction(long auctionId);   
}
