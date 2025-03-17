namespace Domain.Shared;

public class StartAuction(List<long> vehicleIds,
                          DateTime endDate) : Command
{
    public List<long> VehicleIds { get; } = vehicleIds;
    public DateTime EndDate { get; } = endDate;

    public override bool IsValid()
    {
        //NICE_TO_HAVE: Implement command validations
        return true;
    }
}
