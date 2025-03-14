namespace Domain.Shared;

public interface IAuctionsService
{    
    AuctionInfo StartAuction(StartAuction command);
    AuctionInfo EndAuction(EndAuction command);
    BidInfo? PlaceBid(PlaceBid command); 
}
