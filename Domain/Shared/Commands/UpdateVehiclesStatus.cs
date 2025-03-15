namespace Domain.Shared;

public class UpdateVehiclesStatus
{
    public List<long> VehicleIds { get; set; } = [];
    public VehicleStatuses Status { get; set; }
}
