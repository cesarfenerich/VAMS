using Domain.Shared;

namespace Domain.Auctions;

internal class AuctionsHandler(IAuctionsCommandService auctionsService) : IAuctionsHandler
{
    private readonly IAuctionsCommandService _auctionsService = auctionsService;

    public void Handle(StartAuction command)
    {
        _auctionsService.StartAuction(command);
    }

    public void Handle(PlaceBid command)
    {
        _auctionsService.PlaceBid(command);
    }

    public void Handle(EndAuction command)
    {
        _auctionsService.EndAuction(command);
    }
}
