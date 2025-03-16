using Domain.Shared;

namespace Domain.Auctions;

internal readonly struct Bid
{    
    public decimal Amount { get; }    

    public Bid(decimal amount, decimal startingBid, List<Bid> bids)
    {
        if (amount < 0)
            throw new AuctionsException($"Bid amount ({amount}) cannot be negative.");

        if (bids.Count == 0 && amount < startingBid)
            throw new AuctionsException($"Bid amount ({amount}) must be greater than the vehicle starting bid ({startingBid}).");

        if (bids.Any(x => x.Amount >= amount))
            throw new AuctionsException($"Bid amount ({amount}) must be greater than the current highest bid ({bids.Max(x => x.Amount)}).");
      
        Amount = amount;      
    }  

    public BidInfo AsModel(decimal winnerBid)
    {
        return new BidInfo()
        {           
            Amount = Amount,
            IsWinningBid = Amount >= winnerBid
        };
    }
}
