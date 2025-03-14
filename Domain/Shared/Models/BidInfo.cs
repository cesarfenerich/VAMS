namespace Domain.Shared;

public class BidInfo
{
    public long Id { get; set; }

    public VehicleInfo Vehicle { get; set; }

    public decimal Amount { get; set; } 

    public bool IsWinning { get; set; }    
    
}