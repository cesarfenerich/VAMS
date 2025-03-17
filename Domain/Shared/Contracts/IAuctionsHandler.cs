namespace Domain.Shared;

public interface IAuctionsHandler
{    
    void Handle(StartAuction command);
    void Handle(PlaceBid command);
    void Handle(EndAuction command);  
}
