namespace Domain.Shared;

public interface IAuctionsCommandService
{
    void StartAuction(StartAuction command);
    void PlaceBid(PlaceBid command);
    void EndAuction(EndAuction command);
}
