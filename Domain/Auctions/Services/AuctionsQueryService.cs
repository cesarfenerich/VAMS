using Domain.Shared;

namespace Domain.Auctions;

internal class AuctionsQueryService(IAuctionsRepository auctionsRepository) : IAuctionsQueryService
{
    private readonly IAuctionsRepository _auctions = auctionsRepository;

    public AuctionInfo GetAuctionById(long id)
    {
        var auction = _auctions.GetById(id)?.AsModel();

        return auction ?? throw new AuctionsException($"Auction with id {id} not found.");
    }

    public AuctionsView GetAuctions()
        => new(_auctions.GetAll().Select(x => x.AsModel()).ToList());    
}
