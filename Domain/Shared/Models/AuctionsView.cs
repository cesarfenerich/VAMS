namespace Domain.Shared;

public class AuctionsView(IEnumerable<AuctionInfo> auctions)
{
    public IEnumerable<AuctionInfo> Auction { get; private set; } = auctions;
}
