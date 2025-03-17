namespace Domain.Shared;

public class AuctionsView(List<AuctionInfo> auctions)
{
    public List<AuctionInfo> Auctions { get; set; } = auctions;
}
