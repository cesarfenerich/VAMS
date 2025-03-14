namespace Domain.Shared;

public class VehiclesView(IEnumerable<VehicleInfo> vehicles)
{
    public IEnumerable<VehicleInfo> Vehicles { get; private set; } = vehicles;
}
